using Discord;
using Discord.Interactions;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        public partial class Display
        {
            [SlashCommand("progress", "Control how progress command responses should look")]
            public async Task<RuntimeResult> SetProgress(ProgressDisplayType type)
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
                if (config is null)
                    return await Response.Fail(T("error.noSuchConfig", lng), interaction);

                config.ProgressDisplay = type;

                Log.Info(
                    $"Updated configuration for guild {config.GuildId}, set Progress Display to {type.ToFriendlyString(lng)}"
                );

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.serverConfiguration", lng))
                    .WithDescription(T("server.configuration.saved", lng))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
