using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        public partial class Admin
        {
            [SlashCommand("remove", "Remove an administrator from this server")]
            public async Task<RuntimeResult> Remove(
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

                // Validate user is an admin
                var admin = config.Administrators.FirstOrDefault(a => a.UserId == memberId);
                if (admin is null)
                    return await Response.Fail(T("error.noSuchAdmin", lng, staffMention), interaction);

                config.Administrators.Remove(admin);

                Log.Info($"Updated configuration for guild {config.GuildId}, removed {memberId} as an administrator");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.serverConfiguration", lng))
                    .WithDescription(T("server.admin.removed", lng, staffMention))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
