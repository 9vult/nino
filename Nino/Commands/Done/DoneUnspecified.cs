using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using NaturalSort.Extension;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Done
    {
        private async Task<RuntimeResult> HandleUnspecified(SocketInteraction interaction, Project project, string abbreviation, InteractiveService interactiveService)
        {
            Log.Info($"Handling unspecified /done by M[{interaction.User.Id} (@{interaction.User.Username})] for {project}");
            
            var lng = interaction.UserLocale;

            var episodes = project.Episodes.OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort()).ToList();

            // Find the episode the team is working on
            var workingEpisodeNo = episodes.FirstOrDefault(e => !e.Done)?.Number ?? episodes.LastOrDefault()?.Number;
            if (workingEpisodeNo == null)
                return await Response.Fail(T("error.noIncompleteEpisodes", lng), interaction);

            // Find the next episode awaiting this task's completion
            var nextTaskEpisodeNo = episodes.FirstOrDefault(e => e.Tasks.Any(t => t.Abbreviation == abbreviation && !t.Done))?.Number;
            if (nextTaskEpisodeNo == null)
            {
                // We do a little research
                if (episodes.Any(e => e.Tasks.Any(t => t.Abbreviation == abbreviation)))
                    return await Response.Fail(T("error.taskCompleteAllEpisodes", lng), interaction);
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);
            }
            
            // Are they the same? Then hand it off to the Specified handler
            if (nextTaskEpisodeNo == workingEpisodeNo)
                return await HandleSpecified(interaction, project, abbreviation, workingEpisodeNo);

            // We are working ahead

            // Verify user
            project.TryGetEpisode(nextTaskEpisodeNo, out var nextTaskEpisode);

            if (!Utils.VerifyTaskUser(interaction.User.Id, project, nextTaskEpisode!, abbreviation))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            var role = project.KeyStaff.Concat(nextTaskEpisode!.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation).Role;

            // How to proceed question embed
            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var questionBody = T("progress.done.inTheDust", lng, workingEpisodeNo, role.Name, nextTaskEpisodeNo);
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(questionBody)
                .WithCurrentTimestamp()
                .Build();
            var component = new ComponentBuilder()
                .WithButton(T("progress.done.inTheDust.dontDoIt.button", lng), "ninodonecancel", ButtonStyle.Danger)
                .WithButton(T("progress.done.inTheDust.doItNow.button", lng), "ninodoneproceed", ButtonStyle.Success)
                .Build();
            var questionResponse = await interaction.FollowupAsync(embed: questionEmbed, components: component);

            // Wait for response
            var questionResult = await interactiveService.NextMessageComponentAsync(
                m => m.Message.Id == questionResponse.Id, timeout: TimeSpan.FromSeconds(60));

            var fullSend = false;
            string finalBody;
            if (!questionResult.IsSuccess)
                finalBody = T("progress.done.inTheDust.timeout", lng);
            else
            {
                if (questionResult.Value.Data.CustomId == "ninodonecancel")
                    finalBody = T("progress.done.inTheDust.dontDoIt", lng);
                else
                {
                    fullSend = true;
                    var nextTaskIndex = episodes.FindIndex(e => e.Number == nextTaskEpisodeNo);
                    var workingIndex = episodes.FindIndex(e => e.Number == workingEpisodeNo);
                    var diff = nextTaskIndex - workingIndex;
                    Dictionary<string, object> map = new() { ["taskName"] = role.Name, ["count"] = diff };
                    finalBody = T("progress.done.inTheDust.doItNow", lng, args: map, pluralName: "count");
                }
            }

            // Update the question embed to replect the choice
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

            // If we're continuing, hand off processing to the Specified handler
            if (fullSend)
                return await HandleSpecified(interaction, project, abbreviation, nextTaskEpisodeNo);
            
            return ExecutionResult.Success;     
        }
    }
}
