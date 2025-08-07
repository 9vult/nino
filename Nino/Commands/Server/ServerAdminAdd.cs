using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        public partial class Admin
        {
            [SlashCommand("add", "Add an administrator to this server")]
            public async Task<RuntimeResult> Add(
                [Summary("member", "Staff member")] SocketGuildUser member
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;
                var guildId = interaction.GuildId ?? 0;
                var guild = Nino.Client.GetGuild(guildId);

                // Validate inputs
                var memberId = member.Id;
                var staffMention = $"<@{memberId}>";

                // Server administrator permissions required
                var runner = guild.GetUser(interaction.User.Id);
                if (!Utils.VerifyAdministrator(runner, guild, excludeServerAdmins: true))
                    return await Response.Fail(T("error.notPrivileged", lng), interaction);

                var config = db.GetConfig(guildId);
                if (config is null)
                    return await Response.Fail(T("error.noSuchConfig", lng), interaction);

                // Validate user isn't already an admin
                if (config.Administrators.Any(a => a.UserId == memberId))
                    return await Response.Fail(T("error.admin.alreadyAdmin", lng, staffMention), interaction);

                // Add to database
                config.Administrators.Add(new Administrator
                {
                    UserId = memberId,
                });

                Log.Info($"Updated configuration for guild {config.GuildId}, added {memberId} as an administrator");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.serverConfiguration", lng))
                    .WithDescription(T("server.admin.added", lng, staffMention))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
