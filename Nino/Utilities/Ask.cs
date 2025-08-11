using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Nino.Records;
using Nino.Records.Enums;
using static Localizer.Localizer;

namespace Nino.Utilities
{
    internal static class Ask
    {
        public static async Task<(bool, string)> AboutIrreversibleAction(
            InteractiveService interactive,
            SocketInteraction interaction,
            Project project,
            string lng,
            IrreversibleAction action
        )
        {
            var questionBodyKey = action switch
            {
                IrreversibleAction.Archive => "project.archive.question",
                IrreversibleAction.Delete => "project.delete.question",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
            };
            var finalBodyKey = action switch
            {
                IrreversibleAction.Archive => "project.archive.done",
                IrreversibleAction.Delete => "project.deleted",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
            };

            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var component = new ComponentBuilder()
                .WithButton(
                    T("project.archive.cancel.button", lng),
                    "ninoarchivecancel",
                    ButtonStyle.Danger
                )
                .WithButton(
                    T("project.archive.continue.button", lng),
                    "ninoarchivecontinue",
                    ButtonStyle.Secondary
                )
                .Build();
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(T(questionBodyKey, lng, project.Nickname))
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

            var goOn = false;
            string finalBody;

            if (!questionResult.IsSuccess)
                finalBody = T("progress.done.inTheDust.timeout", lng);
            else
            {
                if (questionResult.Value.Data.CustomId == "ninoarchivecancel")
                    finalBody = T("progress.done.inTheDust.dontDoIt", lng);
                else
                {
                    goOn = true;
                    finalBody = T(finalBodyKey, lng, project.Nickname);
                }
            }

            return (goOn, finalBody);
        }

        public static async Task<(bool, string, RestFollowupMessage?)> AboutAction(
            InteractiveService interactive,
            SocketInteraction interaction,
            Project project,
            string lng,
            InconsequentialAction inconsequentialAction
        )
        {
            var questionBodyKey = inconsequentialAction switch
            {
                InconsequentialAction.PingCongaAfterSkip => "progress.skip.conga.question",
                InconsequentialAction.MarkTaskDoneIfEpisodeIsDone =>
                    "keyStaff.add.markDone.question",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(inconsequentialAction),
                    inconsequentialAction,
                    null
                ),
            };
            var finalBodyKey = inconsequentialAction switch
            {
                InconsequentialAction.PingCongaAfterSkip => string.Empty,
                InconsequentialAction.MarkTaskDoneIfEpisodeIsDone =>
                    "keyStaff.add.markDone.response",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(inconsequentialAction),
                    inconsequentialAction,
                    null
                ),
            };
            var leftButton = inconsequentialAction switch
            {
                InconsequentialAction.PingCongaAfterSkip => (
                    "progress.skip.conga.no.button",
                    ButtonStyle.Secondary
                ),
                InconsequentialAction.MarkTaskDoneIfEpisodeIsDone => (
                    "keyStaff.add.markDone.no.button",
                    ButtonStyle.Secondary
                ),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(inconsequentialAction),
                    inconsequentialAction,
                    null
                ),
            };
            var rightButton = inconsequentialAction switch
            {
                InconsequentialAction.PingCongaAfterSkip => (
                    "progress.skip.conga.yes.button",
                    ButtonStyle.Secondary
                ),
                InconsequentialAction.MarkTaskDoneIfEpisodeIsDone => (
                    "keyStaff.add.markDone.yes.button",
                    ButtonStyle.Secondary
                ),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(inconsequentialAction),
                    inconsequentialAction,
                    null
                ),
            };
            var successOnTimeout = inconsequentialAction switch
            {
                InconsequentialAction.PingCongaAfterSkip => true,
                InconsequentialAction.MarkTaskDoneIfEpisodeIsDone => true,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(inconsequentialAction),
                    inconsequentialAction,
                    null
                ),
            };

            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var component = new ComponentBuilder()
                .WithButton(T(leftButton.Item1, lng), "ninoinconqcancel", leftButton.Item2)
                .WithButton(T(rightButton.Item1, lng), "ninoinconqcontinue", rightButton.Item2)
                .Build();
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(T(questionBodyKey, lng, project.Nickname))
                .WithCurrentTimestamp()
                .Build();

            var questionResponse = await interaction.FollowupAsync(
                embed: questionEmbed,
                components: component
            );

            // Wait for response
            var questionResult = await interactive.NextMessageComponentAsync(
                m => m.Message.Id == questionResponse.Id,
                timeout: TimeSpan.FromSeconds(60)
            );

            var goOn = false;
            var finalBody = string.Empty;

            if (!questionResult.IsSuccess)
            {
                finalBody = T("progress.done.inTheDust.timeout", lng);
            }
            else
            {
                if (questionResult.Value.Data.CustomId == "ninoinconqcancel")
                    finalBody = T("progress.done.inTheDust.dontDoIt", lng);
                else
                {
                    goOn = true;
                    if (!string.IsNullOrEmpty(finalBodyKey))
                        finalBody = T(finalBodyKey, lng, project.Nickname);
                }
            }

            return (goOn, finalBody, questionResponse);
        }

        internal enum IrreversibleAction
        {
            Archive,
            Delete,
        }

        internal enum InconsequentialAction
        {
            PingCongaAfterSkip,
            MarkTaskDoneIfEpisodeIsDone,
        }
    }
}
