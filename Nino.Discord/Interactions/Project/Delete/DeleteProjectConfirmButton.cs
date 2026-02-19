// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [ComponentInteraction("nino:project:delete:cancel:*:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> CancelDeleteAsync(Guid projectId, Guid userId)
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

        logger.LogTrace("Project deletion canceled by {UserId}", userId);

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.delete.title", locale))
            .WithDescription(T("action.canceled", locale))
            .WithCurrentTimestamp()
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = null;
        });

        return ExecutionResult.Success;
    }
}
