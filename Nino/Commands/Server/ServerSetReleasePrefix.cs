using Discord;
using Discord.Interactions;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        [SlashCommand("release-prefix", "Specify a prefix for releases")]
        public async Task<RuntimeResult> SetReleasePrefix(string? newValue)
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);

            // Server administrator permissions required
            var runner = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(db, runner, guild, excludeServerAdmins: true))
                return await Response.Fail(T("error.notPrivileged", lng), interaction);

            var config = db.GetConfig(guildId);
            if (config == null)
                return await Response.Fail(T("error.noSuchConfig", lng), interaction);

            // Get inputs
            var prefix = newValue == "-" ? null : newValue;

            config.ReleasePrefix = prefix;

            Log.Info(
                $"Updated configuration for guild {config.GuildId}, set ReleasePrefix to {prefix ?? "(empty)"}"
            );

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription(
                    $"{T("server.configuration.saved", lng)}\n{T("info.resettable", lng)}"
                )
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
