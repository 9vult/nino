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
        [SlashCommand("releaseprefix", "Specify a prefix for releases")]
        public async Task<RuntimeResult> SetReleasePrefix(
            [Summary("newvalue", "New Value")] string? newValue
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);

            // Server administrator permissions required
            var runner = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(runner, guild, excludeServerAdmins: true))
                return await Response.Fail(T("error.notPrivileged", lng), interaction);

            var config = await Getters.GetConfiguration(guildId);
            if (config == null)
                return await Response.Fail(T("error.noSuchConfig", lng), interaction);

            // Get inputs
            var prefix = newValue == "-" ? null : newValue;

            // Apply change and upsert to database
            config.ReleasePrefix = prefix;

            await AzureHelper.Configurations!.UpsertItemAsync(config);
            log.Info($"Updated configuration for guild {config.GuildId}, set ReleasePrefix to {prefix ?? "(empty)"}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription($"{T("server.configuration.saved", lng)}\n{T("info.resettable", lng)}")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
