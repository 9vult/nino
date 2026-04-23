// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Release;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Release;

public partial class ReleaseModule
{
    [ComponentInteraction("nino.release.cancel:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> CancelReleaseAsync(string rawId)
    {
        if (!StateId.TryParse(rawId, out var stateId))
        {
            logger.LogError("Could not parse state id: {StateId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var command = await stateService.LoadStateAsync<ReleaseCommandBase>(stateId);
        if (command is null)
            return await interaction.FailAsync(T("error.state", locale));

        // Verify button is not being hijacked
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != command.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete state
        await stateService.DeleteStateAsync(stateId);

        // Get project data
        var request = await getProjectDataHandler.HandleAsync(
            new GetGenericProjectDataQuery(command.ProjectId)
        );
        if (!request.IsSuccess)
            return await interaction.FailAsync(ResultStatus.ProjectNotFound, locale);

        var pData = request.Value;

        // Respond
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("release.incomplete.title", locale))
            .WithDescription(T("action.canceled", locale))
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = successEmbed;
            m.Components = null;
        });
        return ExecutionResult.Success;
    }
}
