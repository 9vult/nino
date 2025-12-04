using System.Text;
using Discord.WebSocket;
using Localizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NaturalSort.Extension;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using NLog;
using static Localizer.Localizer;

namespace Nino.Services
{
    internal class CongaReminderService(IServiceProvider services) : BackgroundService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int OneHour = 60 * 60 * 1000;

        private static async Task RemindTardyTasks(DataContext db)
        {
            var targets = await db
                .Projects.Include(p => p.Episodes)
                .Where(p => p.CongaReminderEnabled && !p.IsArchived)
                .ToListAsync();
            foreach (var project in targets)
            {
                if (
                    await Nino.Client.GetChannelAsync((ulong)project.CongaReminderChannelId!)
                    is not SocketTextChannel channel
                )
                    continue;

                var config = db.GetConfig(project.GuildId);
                var gLng = config?.Locale?.ToDiscordLocale() ?? channel.Guild.PreferredLocale;

                var prefixMode = config?.CongaPrefix ?? CongaPrefixType.None;
                var reminderText = new List<string>();

                var sortedEpisodes = project
                    .Episodes.Where(e => !e.Tasks.All(t => t.Done))
                    .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort());

                foreach (var episode in sortedEpisodes)
                {
                    foreach (var abbreviation in CongaHelper.GetTardyTasks(project, episode))
                    {
                        var staff = project
                            .KeyStaff.Concat(project.Episodes.SelectMany(e => e.AdditionalStaff))
                            .FirstOrDefault(ks => ks.Role.Abbreviation == abbreviation);
                        if (staff is null)
                            continue;

                        var current = new StringBuilder();
                        var userId =
                            episode
                                .PinchHitters.FirstOrDefault(t =>
                                    t.Abbreviation == staff.Role.Abbreviation
                                )
                                ?.UserId
                            ?? staff.UserId;
                        var staffMention = $"<@{userId}>";
                        var roleTitle = staff.Role.Name;
                        if (prefixMode != CongaPrefixType.None)
                        {
                            // Using a switch expression in the middle of string interpolation is insane btw
                            current.Append(
                                $"[{prefixMode switch {
                                CongaPrefixType.Nickname => project.Nickname,
                                CongaPrefixType.Title => project.Title,
                                _ => string.Empty
                            }}] "
                            );
                        }

                        current.Append(
                            T(
                                "progress.done.conga.reminder",
                                gLng,
                                staffMention,
                                episode.Number,
                                roleTitle
                            )
                        );
                        reminderText.Add(current.ToString());

                        // Update database with new last-reminded time
                        var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
                        task.LastReminded = DateTimeOffset.UtcNow;
                    }
                }

                if (reminderText.Count <= 0)
                    continue;
                foreach (var chunk in reminderText.Chunk(13)) // Because 12 would cause 3 messages on 25
                {
                    await channel.SendMessageAsync(string.Join(Environment.NewLine, chunk));
                }
                Log.Info($"Published conga reminders for {project}");

                await db.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("Conga Reminder Service started");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(OneHour, stoppingToken);

                try
                {
                    using var scope = services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                    await RemindTardyTasks(db);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}
