using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Episodes
    {
        [SlashCommand("add", "Add an episode")]
        public async Task<RuntimeResult> Add(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number")] string episodeNumber,
            [Summary("quantity", "Number of episodes to add"), MinValue(1)] int quantity = 1
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            alias = alias.Trim();
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);

            if (quantity != 1 && (!Utils.EpisodeNumberIsInteger(episodeNumber, out var episodeNumberInt) || episodeNumberInt < 0))
                return await Response.Fail(T("error.episode.notInteger", lng, alias), interaction);

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode doesn't exist
            if (Getters.TryGetEpisode(project, episodeNumber, out _))
                return await Response.Fail(T("error.episode.alreadyExists", lng, episodeNumber), interaction);

            string successDescription;
            
            // Single episode
            if (quantity == 1)
            {
                // Create episode
                var newEpisode = CreateEpisode(project, episodeNumber);

                // Add to database
                await AzureHelper.Episodes!.UpsertItemAsync(newEpisode);

                Log.Info($"Added episode {newEpisode} to {project}");
                successDescription = T("episode.added", lng, episodeNumber, project.Nickname);
            }
            // Bulk addition
            else
            {
                var allEps = Cache.GetEpisodes(project.Id);
                Utils.EpisodeNumberIsInteger(episodeNumber, out episodeNumberInt);
                var episodeNumbersToAdd = Enumerable.Range(episodeNumberInt, quantity).Where(n => allEps.All(e => e.Number != $"{n}")).ToList();
                
                // Verify user intent
                var questionBody = T("episode.displayListOfCandidates", lng, $"[ {string.Join(", ", episodeNumbersToAdd)} ]");
                var header = project.IsPrivate
                    ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                    : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";
                
                var component = new ComponentBuilder()
                    .WithButton(T("progress.done.inTheDust.dontDoIt.button", lng), "ninoepisodeaddcancel", ButtonStyle.Danger)
                    .WithButton(T("progress.done.inTheDust.doItNow.button", lng), "ninoepisodeadproceed", ButtonStyle.Success)
                    .Build();
                var questionEmbed = new EmbedBuilder()
                    .WithAuthor(header)
                    .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                    .WithDescription(questionBody)
                    .WithCurrentTimestamp()
                    .Build();
                
                var questionResponse = await interaction.ModifyOriginalResponseAsync(m => {
                    m.Embed = questionEmbed;
                    m.Components = component;
                });
                
                // Wait for response
                var questionResult = await _interactiveService.NextMessageComponentAsync(
                    m => m.Message.Id == questionResponse.Id, timeout: TimeSpan.FromSeconds(60));
                
                var fullSend = false;
                string finalBody;
                
                if (!questionResult.IsSuccess)
                    finalBody = T("progress.done.inTheDust.timeout", lng);
                else
                {
                    if (questionResult.Value.Data.CustomId == "ninoepisodeaddcancel")
                        finalBody = T("episode.add.cancel.response", lng);
                    else
                    {
                        fullSend = true;
                        var bodyDict = new Dictionary<string, object>() { ["number"] = episodeNumbersToAdd.Count };
                        finalBody = T("episode.add.proceed.response", lng, bodyDict);
                    }
                }
                
                // Update the question embed to reflect the choice
                var editedEmbed = new EmbedBuilder()
                    .WithAuthor(header)
                    .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                    .WithDescription(finalBody)
                    .WithCurrentTimestamp()
                    .Build();
                
                await questionResponse.ModifyAsync(m => {
                    m.Components = null;
                    m.Embed = editedEmbed;
                });

                if (!fullSend) return ExecutionResult.Success;

                var bulkEpisodes = episodeNumbersToAdd
                    .Select(n => $"{n}")
                    .Select(n => CreateEpisode(project, n))
                    .ToList();
                
                TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.Id.ToString()));
                foreach (var episode in bulkEpisodes)
                {
                    batch.UpsertItem(episode);
                }
                await batch.ExecuteAsync();
                
                Log.Info($"Added {bulkEpisodes.Count} episodes to {project}");
                var replyDict = new Dictionary<string, object>
                    { ["number"] = bulkEpisodes.Count, ["project"] = project.Nickname };
                successDescription = T("episode.added.bulk", lng, replyDict);
            }

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(successDescription)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
        
        /// <summary>
        /// Episode factory method
        /// </summary>
        /// <param name="project">Project the episode is for</param>
        /// <param name="episodeNumber">Episode number</param>
        /// <returns>New episode</returns>
        private static Episode CreateEpisode (Project project, string episodeNumber)
        {
            return new Episode
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
        }
    }
}
