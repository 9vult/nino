// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.Project.Delete;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [ComponentInteraction("nino.project.delete.cancel:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> CancelDeleteAsync(string id)
    {
        var stateId = Guid.Parse(id);
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var commandDto = await stateService.LoadStateAsync<DeleteProjectCommand>(stateId);
        if (commandDto is null)
            return await interaction.FailAsync(T("error.db", locale));

        // Verify button was clicked by initiator
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != commandDto.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete state, won't be needed regardless of the final status
        await stateService.DeleteStateAsync(stateId);

        logger.LogTrace("Project deletion canceled by {UserId}", commandDto.RequestedBy);

        var data = await dataService.GetProjectBasicInfoAsync(commandDto.ProjectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("action.question", locale))
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
