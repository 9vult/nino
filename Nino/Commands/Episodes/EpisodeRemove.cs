using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Episodes
    {
        [SlashCommand("remove", "Remove an episode")]
        public async Task<RuntimeResult> Remove(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Autocomplete(typeof(EpisodeAutocompleteHandler))] string? lastEpisodeNumber = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeNumber);

            if (lastEpisodeNumber is not null)
                lastEpisodeNumber = Episode.CanonicalizeEpisodeNumber(lastEpisodeNumber);

            // Verify project and user - Owner or Admin required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode exists
            if (!project.TryGetEpisode(episodeNumber, out var episode))
                return await Response.Fail(
                    T("error.noSuchEpisode", lng, episodeNumber),
                    interaction
                );

            string successDescription;

            // Single episode removal
            if (lastEpisodeNumber is null)
            {
                project.Episodes.Remove(episode);
                db.Episodes.Remove(episode);

                Log.Info($"Deleted episode {episode} from {project}");
                successDescription = T("episode.removed", lng, episodeNumber, project.Nickname);
            }
            // Multiple episode removal
            else
            {
                if (!project.TryGetEpisode(lastEpisodeNumber, out _))
                    return await Response.Fail(
                        T("error.noSuchEpisode", lng, lastEpisodeNumber),
                        interaction
                    );

                var episodes = project
                    .Episodes.OrderBy(
                        e => e.Number,
                        StringComparison.OrdinalIgnoreCase.WithNaturalSort()
                    )
                    .ToList();
                var startIndex = episodes.FindIndex(ep => ep.Number == episodeNumber);
                var endIndex = episodes.FindIndex(ep => ep.Number == lastEpisodeNumber);

                if (
                    startIndex == -1
                    || endIndex == -1
                    || startIndex > endIndex
                    || startIndex == endIndex
                )
                    return await Response.Fail(
                        T("error.invalidEpisodeRange", lng, episodeNumber, lastEpisodeNumber),
                        interaction
                    );

                var bulkEpisodes = episodes
                    .Skip(startIndex)
                    .Take(endIndex - startIndex + 1)
                    .ToList();
                var episodesToRemove = bulkEpisodes.Select(e => e.Number).ToList();

                // Verify user intent
                var questionBody = T(
                    "episode.displayListOfRemovalCandidates",
                    lng,
                    $"[ {string.Join(", ", episodesToRemove)} ]"
                );
                var header = project.IsPrivate
                    ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                    : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

                var component = new ComponentBuilder()
                    .WithButton(
                        T("progress.done.inTheDust.dontDoIt.button", lng),
                        "ninoepisoderemcancel",
                        ButtonStyle.Secondary
                    )
                    .WithButton(
                        T("progress.done.inTheDust.doItNow.button", lng),
                        "ninoepisoderemproceed",
                        ButtonStyle.Danger
                    )
                    .Build();
                var questionEmbed = new EmbedBuilder()
                    .WithAuthor(header)
                    .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                    .WithDescription(questionBody)
                    .WithCurrentTimestamp()
                    .Build();

                var questionResponse = await interaction.ModifyOriginalResponseAsync(m =>
                {
                    m.Embed = questionEmbed;
                    m.Components = component;
                });

                // Wait for response
                var questionResult = await interactive.NextMessageComponentAsync(
                    m => m.Message.Id == questionResponse.Id,
                    timeout: TimeSpan.FromSeconds(60)
                );

                var fullSend = false;
                string finalBody;

                if (!questionResult.IsSuccess)
                    finalBody = T("progress.done.inTheDust.timeout", lng);
                else
                {
                    if (questionResult.Value.Data.CustomId == "ninoepisoderemcancel")
                        finalBody = T("episode.remove.cancel.response", lng);
                    else
                    {
                        fullSend = true;
                        var bodyDict = new Dictionary<string, object>
                        {
                            ["number"] = episodesToRemove.Count,
                        };
                        finalBody = T("episode.remove.proceed.response", lng, bodyDict);
                    }
                }

                // Update the question embed to reflect the choice
                var editedEmbed = new EmbedBuilder()
                    .WithAuthor(header)
                    .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                    .WithDescription(finalBody)
                    .WithCurrentTimestamp()
                    .Build();

                await questionResponse.ModifyAsync(m =>
                {
                    m.Components = null;
                    m.Embed = editedEmbed;
                });

                if (!fullSend)
                    return ExecutionResult.Success;

                project.Episodes.RemoveAll(e => bulkEpisodes.Contains(e));
                db.Episodes.RemoveRange(bulkEpisodes);

                Log.Info($"Removed {bulkEpisodes.Count} episodes from {project}");
                var replyDict = new Dictionary<string, object>
                {
                    ["number"] = bulkEpisodes.Count,
                    ["project"] = project.Nickname,
                };
                successDescription = T("episode.removed.bulk", lng, replyDict);
            }

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(successDescription)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
