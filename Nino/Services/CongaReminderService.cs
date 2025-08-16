using System.Text;
using Discord.WebSocket;
using Localizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                var reminderText = new StringBuilder();
                foreach (var episode in project.Episodes.Where(e => !e.Tasks.All(t => t.Done)))
                {
                    foreach (var abbreviation in CongaHelper.GetTardyTasks(project, episode))
                    {
                        var staff = project
                            .KeyStaff.Concat(project.Episodes.SelectMany(e => e.AdditionalStaff))
                            .FirstOrDefault(ks => ks.Role.Abbreviation == abbreviation);
                        if (staff is null)
                            continue;

                        var userId =
                            episode
                                .PinchHitters.FirstOrDefault(t =>
                                    t.Abbreviation == staff.Role.Abbreviation
                                )
                                ?.UserId ?? staff.UserId;
                        var staffMention = $"<@{userId}>";
                        var roleTitle = staff.Role.Name;
                        if (prefixMode != CongaPrefixType.None)
                        {
                            // Using a switch expression in the middle of string interpolation is insane btw
                            reminderText.Append(
                                $"[{prefixMode switch {
                                CongaPrefixType.Nickname => project.Nickname,
                                CongaPrefixType.Title => project.Title,
                                _ => string.Empty
                            }}] "
                            );
                        }

                        reminderText.AppendLine(
                            T(
                                "progress.done.conga.reminder",
                                gLng,
                                staffMention,
                                episode.Number,
                                roleTitle
                            )
                        );

                        // Update database with new last-reminded time
                        var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
                        task.LastReminded = DateTimeOffset.UtcNow;
                    }
                }

                if (reminderText.Length <= 0)
                    continue;
                await channel.SendMessageAsync(reminderText.ToString());
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
