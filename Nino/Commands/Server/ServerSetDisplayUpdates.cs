using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        public partial class Display
        {
            [SlashCommand("updates", "Control how progress updates should look")]
            public async Task<RuntimeResult> SetUpdates(
                [Summary("type", "Display type")] UpdatesDisplayType type
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

                // Apply change and upsert to database
                config.UpdateDisplay = type;

                await AzureHelper.Configurations!.UpsertItemAsync(config);
                log.Info($"Updated configuration for guild {config.GuildId}, set Update Display to {type.ToFriendlyString(lng)}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.serverConfiguration", lng))
                    .WithDescription(T("server.configuration.saved", lng))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                return ExecutionResult.Success;
            }
        }
    }
}
