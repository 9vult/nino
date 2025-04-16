using Discord;
using Discord.WebSocket;
using Fergun.Interactive;
using Nino.Records;
using Nino.Records.Enums;
using static Localizer.Localizer;

namespace Nino.Utilities
{
    internal static class Ask
    {
        public static async Task<(bool, string)> AboutIrreversibleAction(InteractiveService interactive,
            SocketInteraction interaction, Project project, string lng, IrreversibleAction action)
        {
            var questionBodyKey = action switch
            {
                IrreversibleAction.Archive => "project.archive.question",
                IrreversibleAction.Delete => "project.delete.question",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
            var finalBodyKey = action switch
            {
                IrreversibleAction.Archive => "project.archive.done",
                IrreversibleAction.Delete => "project.deleted",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var component = new ComponentBuilder()
                .WithButton(T("project.archive.cancel.button", lng), "ninoarchivecancel", ButtonStyle.Danger)
                .WithButton(T("project.archive.continue.button", lng), "ninoarchivecontinue", ButtonStyle.Secondary)
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
                m => m.Message.Id == questionResponse.Id, timeout: TimeSpan.FromSeconds(60));

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

        internal enum IrreversibleAction
        {
            Archive,
            Delete
        }
    }
}