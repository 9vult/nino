using Discord;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Observer
    {
        public static async Task<bool> HandleAdd(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;
            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var originGuildIdStr = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "serverid")).Trim();
            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            var blame = (bool)subcommand.Options.FirstOrDefault(o => o.Name == "done")!.Value;
            var updatesUrl = (string?)(subcommand.Options.FirstOrDefault(o => o.Name == "updates")?.Value);
            var releasesUrl = (string?)(subcommand.Options.FirstOrDefault(o => o.Name == "releases")?.Value);
            var roleId = ((SocketRole?)subcommand.Options.FirstOrDefault(o => o.Name == "member")?.Value)?.Id;

            // Validate no-op condition
            if (!blame && updatesUrl == null && releasesUrl == null)
                return await Response.Fail(T("error.observerNoOp", lng), interaction);

            // Validate observer server
            if (!ulong.TryParse(originGuildIdStr, out var originGuildId))
                return await Response.Fail(T("error.invalidServerId", lng), interaction);
            var originGuild = Nino.Client.GetGuild(originGuildId);
            if (originGuild == null)
                return await Response.Fail(T("error.noSuchServer", lng), interaction);

            // Verify project and user access
            var project = await Utils.ResolveAlias(alias, interaction, observingGuildId: originGuildId);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            // Fake project existence if private
            if (project.IsPrivate && !Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var observer = new Records.Observer
            {
                Id = $"{project.Id}-{guildId}",
                GuildId = guildId,
                OriginGuildId = originGuildId,
                ProjectId = project.Id,
                Blame = blame,
                ProgressWebhook = updatesUrl,
                ReleasesWebhook = releasesUrl,
                RoleId = roleId
            };

            // Add to database
            await AzureHelper.Observers!.UpsertItemAsync(observer);

            log.Info($"Created new observer {observer.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.observer", lng))
                .WithDescription(T("observer.added", lng, project.Nickname, originGuild.Name))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildObserverCache();
            return true;
        }
    }
}
