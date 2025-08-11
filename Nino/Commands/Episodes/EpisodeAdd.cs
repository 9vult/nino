using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;
using Task = Nino.Records.Task;

namespace Nino.Commands
{
    public partial class Episodes
    {
        [SlashCommand("add", "Add an episode")]
        public async Task<RuntimeResult> Add(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [MaxLength(32)] string episodeNumber,
            [MinValue(1)] int quantity = 1
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeNumber);

            if (
                quantity != 1
                && (
                    !Episode.EpisodeNumberIsInteger(episodeNumber, out var episodeNumberInt)
                    || episodeNumberInt < 0
                )
            )
                return await Response.Fail(T("error.episode.notInteger", lng, alias), interaction);

            // Verify project and user - Owner or Admin required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode doesn't exist
            if (project.TryGetEpisode(episodeNumber, out _))
                return await Response.Fail(
                    T("error.episode.alreadyExists", lng, episodeNumber),
                    interaction
                );

            string successDescription;

            // Single episode
            if (quantity == 1)
            {
                // Create episode
                var newEpisode = CreateEpisode(project, episodeNumber);
                project.Episodes.Add(newEpisode);

                Log.Info($"Added episode {newEpisode} to {project}");
                successDescription = T("episode.added", lng, episodeNumber, project.Nickname);
            }
            // Bulk addition
            else
            {
                Episode.EpisodeNumberIsInteger(episodeNumber, out episodeNumberInt);
                var episodeNumbersToAdd = Enumerable
                    .Range(episodeNumberInt, quantity)
                    .Where(n => project.Episodes.All(e => e.Number != $"{n}"))
                    .ToList();

                // Verify user intent
                var questionBody = T(
                    "episode.displayListOfCandidates",
                    lng,
                    $"[ {string.Join(", ", episodeNumbersToAdd)} ]"
                );
                var header = project.IsPrivate
                    ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                    : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

                var component = new ComponentBuilder()
                    .WithButton(
                        T("progress.done.inTheDust.dontDoIt.button", lng),
                        "ninoepisodeaddcancel",
                        ButtonStyle.Secondary
                    )
                    .WithButton(
                        T("progress.done.inTheDust.doItNow.button", lng),
                        "ninoepisodeadproceed",
                        ButtonStyle.Success
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
                    if (questionResult.Value.Data.CustomId == "ninoepisodeaddcancel")
                        finalBody = T("episode.add.cancel.response", lng);
                    else
                    {
                        fullSend = true;
                        var bodyDict = new Dictionary<string, object>
                        {
                            ["number"] = episodeNumbersToAdd.Count,
                        };
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

                await questionResponse.ModifyAsync(m =>
                {
                    m.Components = null;
                    m.Embed = editedEmbed;
                });

                if (!fullSend)
                    return ExecutionResult.Success;

                var bulkEpisodes = episodeNumbersToAdd
                    .Select(n => $"{n}")
                    .Select(n => CreateEpisode(project, n))
                    .ToList();
                project.Episodes.AddRange(bulkEpisodes);

                Log.Info($"Added {bulkEpisodes.Count} episodes to {project}");
                var replyDict = new Dictionary<string, object>
                {
                    ["number"] = bulkEpisodes.Count,
                    ["project"] = project.Nickname,
                };
                successDescription = T("episode.added.bulk", lng, replyDict);
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

        /// <summary>
        /// Episode factory method
        /// </summary>
        /// <param name="project">Project the episode is for</param>
        /// <param name="episodeNumber">Episode number</param>
        /// <returns>New episode</returns>
        private static Episode CreateEpisode(Project project, string episodeNumber)
        {
            return new Episode
            {
                GuildId = project.GuildId,
                ProjectId = project.Id,
                Number = episodeNumber,
                Done = false,
                ReminderPosted = false,
                AdditionalStaff = [],
                PinchHitters = [],
                Tasks = project
                    .KeyStaff.Select(ks => new Task
                    {
                        Abbreviation = ks.Role.Abbreviation,
                        Done = false,
                    })
                    .ToList(),
            };
        }
    }
}
