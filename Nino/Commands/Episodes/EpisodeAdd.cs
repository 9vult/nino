using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Episodes
    {
        [SlashCommand("add", "Add an episode")]
        public async Task<RuntimeResult> Add(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number")] string episodeNumber
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode doesn't exist
            if (Getters.TryGetEpisode(project, episodeNumber, out _))
                return await Response.Fail(T("error.episode.alreadyExists", lng, episodeNumber), interaction);

            // Create episode
            var newEpisode = new Episode
            {
                Id = AzureHelper.CreateEpisodeId(),
                GuildId = project.GuildId,
                ProjectId = project.Id,
                Number = episodeNumber,
                Done = false,
                ReminderPosted = false,
                AdditionalStaff = [],
                PinchHitters = [],
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
            return ExecutionResult.Success;
        }
    }
}
