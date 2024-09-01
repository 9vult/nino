using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Done
    {
        public static async Task<RuntimeResult> HandleUnspecified(SocketSlashCommand interaction, Project project, string abbreviation)
        {
            var lng = interaction.UserLocale;

            var episodes = Cache.GetEpisodes(project.Id);

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
                return await HandleSpecified(interaction, project, abbreviation, (decimal)workingEpisodeNo);

            // We are working ahead

            // Verify user
            var nextTaskEpisode = await Getters.GetEpisode(project, (decimal)nextTaskEpisodeNo);

            if (!Utils.VerifyTaskUser(interaction.User.Id, project, nextTaskEpisode!, abbreviation))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            var task = nextTaskEpisode!.Tasks.First(t => t.Abbreviation == abbreviation);
            var role = project.KeyStaff.Concat(nextTaskEpisode.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation).Role;

            // How to proceed question embed
            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString()})"
                : $"{project.Title} ({project.Type.ToFriendlyString()})";

            var questionBody = T("progress.done.inTheDust", lng, workingEpisodeNo, role.Name, nextTaskEpisodeNo);
            // var proceed = new ButtonBuilder()
            //     .WithCustomId("ninodoneproceed")
            //     .WithLabel(T("progress.done.inTheDust.doItNow.button", lng))
            //     .WithStyle(ButtonStyle.Danger)
            //     .Build();
            // var cancel = new ButtonBuilder()
            //     .WithCustomId("ninodonecancel")
            //     .WithLabel(T("progress.done.inTheDust.dontDoIt.button", lng))
            //     .WithStyle(ButtonStyle.Secondary)
            //     .Build();
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(questionBody)
                .WithCurrentTimestamp()
                .Build();
            // var component = new ComponentBuilder()
            //     .AddRow(new ActionRowBuilder()
            //         .AddComponent(proceed)
            //         .AddComponent(cancel)
            //     ).Build();
            var component = new ComponentBuilder()
                .WithButton(T("progress.done.inTheDust.doItNow.button", lng), "ninodoneproceed")
                .WithButton(T("progress.done.inTheDust.dontDoIt.button", lng), "ninodonecancel")
                .Build();
            var questionResponse = await interaction.ModifyOriginalResponseAsync(m => {
                m.Embed = questionEmbed;
                m.Components = component;
            });

            // Wait for response
            // var questionResult = await Nino.InteractiveService.NextMessageComponentAsync(
            //     m => m.Message.Id == questionResponse.Id, timeout: TimeSpan.FromSeconds(60));

            // bool fullSend = false;
            // string finalBody = string.Empty;
            // if (!questionResult.IsSuccess)
            //     finalBody = T("progress.done.inTheDust.timeout", lng);
            // else
            // {
            //     await questionResult.Value!.DeferAsync();
            //     if (questionResult.Value.Data.CustomId == "ninodonecancel")
            //         finalBody = T("progress.done.inTheDust.dontDoIt", lng);
            //     else
            //     {
            //         fullSend = true;
            //         var diff = Math.Ceiling((decimal)nextTaskEpisodeNo - (decimal)workingEpisodeNo);
            //         Dictionary<string, object> map = new() { ["taskName"] = role.Name, ["count"] = diff };
            //         finalBody = T("progress.done.inTheDust.doItNow", lng, args: map, pluralName: "count");
            //     }
            // }

            // // Update the question embed to replect the choice
            // var editedEmbed = new EmbedBuilder()
            //     .WithAuthor(header)
            //     .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
            //     .WithDescription(finalBody)
            //     .WithCurrentTimestamp()
            //     .Build();

            // await questionResponse.ModifyAsync(m => {
            //     m.Components = null;
            //     m.Embed = editedEmbed;
            // });

            // // If we're continuing, hand off processing to the Specified handler
            // if (fullSend)
            //     return await HandleSpecified(interaction, project, abbreviation, (decimal)nextTaskEpisodeNo);
            
            return ExecutionResult.Success;     
        }
    }
}
