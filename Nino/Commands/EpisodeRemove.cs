using Discord;
using Discord.WebSocket;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class EpisodeManagement
    {
        public static async Task<bool> HandleRemove(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode exists
            var episodeNumber = Convert.ToDecimal(subcommand.Options.FirstOrDefault(o => o.Name == "episode")!.Value);
            var episode = await Getters.GetEpisode(project, episodeNumber);

            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            var episodeId = $"{project.Id}-{episodeNumber}";

            // Add to database
            await AzureHelper.Episodes!.DeleteItemAsync<Episode>(episodeId, AzureHelper.EpisodePartitionKey(episode));

            log.Info($"Deleted episode {episodeNumber} from {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("episode.removed", lng, episodeNumber, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return true;
        }
    }
}
