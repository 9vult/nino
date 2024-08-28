using Discord;
using Discord.WebSocket;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class EpisodeManagement
    {
        public static async Task<bool> HandleAdd(SocketSlashCommand interaction)
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

            // Verify episode doesn't exist
            var episodeNumber = Convert.ToDecimal(subcommand.Options.FirstOrDefault(o => o.Name == "episode")!.Value);
            var episode = await Getters.GetEpisode(project, episodeNumber);

            if (episode != null)
                return await Response.Fail(T("error.episode.alreadyExists", lng, episodeNumber), interaction);

            // Create episode
            var newEpisode = new Episode
            {
                Id = $"{project.Id}-{episodeNumber}",
                GuildId = project.GuildId,
                ProjectId = project.Id,
                Number = episodeNumber,
                Done = false,
                ReminderPosted = false,
                AdditionalStaff = [],
                Tasks = project.KeyStaff.Select(ks => new Records.Task { Abbreviation = ks.Role.Abbreviation, Done = false }).ToArray()
            };

            // Add to database
            await AzureHelper.Episodes!.UpsertItemAsync(newEpisode);

            log.Info($"Added episode {episodeNumber} to {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("episode.added", lng, episodeNumber, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return true;
        }
    }
}
