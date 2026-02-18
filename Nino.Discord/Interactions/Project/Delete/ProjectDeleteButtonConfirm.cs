// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [ComponentInteraction("nino:project:delete:confirm:*:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmDeleteAsync(Guid projectId, Guid userId)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Verify button was clicked by initiator
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != userId
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        var (nickname, title, type, _) = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{title} ({type.ToFriendlyString(locale)})";

        logger.LogTrace("Project deletion confirmed by {UserId}", userId);

        var (status, json) = await deleteHandler.HandleAsync(
            new ProjectDeleteAction(projectId, userId)
        );

        if (status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.projectNotFound", locale),
                    _ => T("error.generic", locale),
                }
            );
        }

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.delete.title", locale))
            .WithDescription(T("project.delete.complete", locale))
            .WithCurrentTimestamp()
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = null;
        });

        var file = new MemoryStream();
        file.Write(Encoding.UTF8.GetBytes(json!));
        file.Position = 0;

        await interaction.FollowupWithFileAsync(
            file,
            $"{nickname}.json",
            T("project.export", locale, nickname)
        );
        return ExecutionResult.Success;
    }
}
