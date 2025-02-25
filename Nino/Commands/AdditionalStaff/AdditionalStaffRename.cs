using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class AdditionalStaff
    {
        [SlashCommand("rename", "Rename an additional staff position")]
        public async Task<RuntimeResult> Rename(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AdditionalStaffAutocompleteHandler))] string abbreviation,
            [Summary("new_abbreviation", "Position shorthand")] string newAbbreviation,
            [Summary("new_name", "Full position name")] string newTaskName
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();
            newTaskName = newTaskName.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            newAbbreviation = newAbbreviation.Trim().ToUpperInvariant();
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            if (!Getters.TryGetEpisode(project, episodeNumber, out var episode))
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            // Check if position exists
            if (episode.AdditionalStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);
            
            // Check if position already exists
            if (abbreviation != newAbbreviation && episode.AdditionalStaff.Any(ks => ks.Role.Abbreviation == newAbbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // Update user
            var updatedStaff = episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation);
            var ksIndex = Array.IndexOf(episode.AdditionalStaff, updatedStaff);
            
            var updatedTask = episode.Tasks.Single(k => k.Abbreviation == abbreviation);
            var taskIndex = Array.IndexOf(episode.Tasks, updatedTask);
            
            updatedStaff.Role.Abbreviation = newAbbreviation;
            updatedStaff.Role.Name = newTaskName;
            
            updatedTask.Abbreviation = newAbbreviation;
            
            // Swap in database
            await AzureHelper.PatchEpisodeAsync(episode, [
                PatchOperation.Replace($"/additionalStaff/{ksIndex}", updatedStaff),
                PatchOperation.Replace($"/tasks/{taskIndex}", updatedTask),
            ]);

            Log.Info($"Renamed task {abbreviation} for episode {episode} to {newAbbreviation} ({newTaskName})");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("additionalStaff.renamed", lng, abbreviation, episode.Number, newAbbreviation, newTaskName))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(episode.ProjectId);
            return ExecutionResult.Success;
        }
    }
}
