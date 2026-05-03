// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Release.Batch;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Discord.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Release;

public partial class ReleaseModule
{
    [ComponentInteraction("nino.release.batch.confirm:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmBatchReleaseAsync(string rawId)
    {
        if (!StateId.TryParse(rawId, out var stateId))
        {
            logger.LogError("Could not parse state id: {StateId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var command = await stateService.LoadStateAsync<ReleaseBatchCommand>(stateId);
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

        var result = await releaseBatchHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(command.ProjectId))
            );
        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(result.Status, locale, new FailureContext());
        }

        var pData = result.Value;
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("release.title", locale))
            .WithDescription(
                T("release.batch.success", locale, command.FirstNumber, command.LastNumber)
            )
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = successEmbed;
            m.Components = null;
        });
        return ExecutionResult.Success;
    }
}
