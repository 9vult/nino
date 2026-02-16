// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("delete", "Delete a project")]
    public async Task<RuntimeResult> DeleteAsync(string alias)
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        // Verify project and user - Owner required
        var (userId, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var projectResolution = await projectResolver.HandleAsync(
            new ProjectResolveAction(alias, groupId, userId)
        );

        if (projectResolution.Status is not ResultStatus.Success)
            return await interaction.FailAsync("");

        var projectId = projectResolution.Data;

        var isVerified = await verificationService.VerifyProjectPermissionsAsync(
            projectId,
            userId,
            PermissionsLevel.Owner
        );
        if (!isVerified)
            return await interaction.FailAsync("");

        // Ask if the user is sure
        var embed = new EmbedBuilder()
            .WithAuthor("Project Name")
            .WithTitle("❓ Are you sure you want to delete this project?")
            .WithDescription("The impostor is suspicious!")
            .WithCurrentTimestamp()
            .Build();

        var cancelId = $"nino:project:delete:cancel:{projectId}:{userId}";
        var confirmId = $"nino:project:delete:confirm:{projectId}:{userId}";

        var component = new ComponentBuilder()
            .WithButton("Cancel", cancelId, ButtonStyle.Danger)
            .WithButton("Confirm", confirmId, ButtonStyle.Secondary)
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = component;
        });

        return ExecutionResult.Success;
    }
}
