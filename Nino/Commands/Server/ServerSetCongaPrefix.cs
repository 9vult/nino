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
        [SlashCommand("conga-prefix", "Set a prefix for Conga notifications")]
        public async Task<RuntimeResult> SetCongaPrefix(CongaPrefixType type)
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

            config.CongaPrefix = type;
            Log.Info(
                $"Updated configuration for guild {config.GuildId}, set Conga Prefix to {type.ToFriendlyString(lng)}"
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
