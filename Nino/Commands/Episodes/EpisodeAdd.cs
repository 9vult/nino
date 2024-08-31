using Discord;
using Discord.Interactions;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Episodes
    {
        [SlashCommand("add", "Add an episode")]
        public async Task<bool> Add(
            [Summary("project", "Project nickname")] string alias,
            [Summary("episode", "Episode number")] decimal episodeNumber
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode doesn't exist
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
