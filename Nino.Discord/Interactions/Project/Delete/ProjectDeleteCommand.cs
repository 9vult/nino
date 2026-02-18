// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("delete", "Delete a project")]
    public async Task<RuntimeResult> DeleteAsync(string alias)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Verify project and user - Owner required
        var (userId, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (status, projectId) = await projectResolver.HandleAsync(
            new ProjectResolveAction(alias, groupId, userId)
        );

        if (status is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var isVerified = await verificationService.VerifyProjectPermissionsAsync(
            projectId,
            userId,
            PermissionsLevel.Owner
        );
        if (!isVerified)
            return await interaction.FailAsync(T("error.permissions", locale));

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        logger.LogTrace("Displaying project deletion confirmation to {UserId}", userId);

        // Ask if the user is sure
        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.delete.title", locale))
            .WithDescription(T("project.delete.question", locale, data.Title))
            .WithCurrentTimestamp()
            .Build();

        var cancelId = $"nino:project:delete:cancel:{projectId}:{userId}";
        var confirmId = $"nino:project:delete:confirm:{projectId}:{userId}";

        var component = new ComponentBuilder()
            .WithButton(T("button.cancel", locale), cancelId, ButtonStyle.Danger)
            .WithButton(T("button.delete", locale), confirmId, ButtonStyle.Secondary)
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = component;
        });

        return ExecutionResult.Success;
    }
}
