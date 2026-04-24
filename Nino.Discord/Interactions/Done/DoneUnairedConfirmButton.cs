// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    [ComponentInteraction("nino.done.unaired.confirm:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmUnairedAsync(string rawId)
    {
        if (!StateId.TryParse(rawId, out var stateId))
        {
            logger.LogError("Could not parse state id: {StateId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var state = await stateService.LoadStateAsync<DoneState>(stateId);
        if (state is null)
            return await interaction.FailAsync(T("error.state", locale));

        // Verify button is not being hijacked
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != state.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete state
        await stateService.DeleteStateAsync(stateId);

        var responseEmbed = new EmbedBuilder()
            .WithProjectInfo(state.ProjectData, locale)
            .WithTitle(T("action.question", locale))
            .WithDescription(T("done.unaired.response", locale))
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = responseEmbed;
            m.Components = null;
        });

        // All good, onward!
        return await HandleFinalAsync(
            interaction,
            state.ProjectData.ProjectId,
            state.TaskData.EpisodeId,
            state.TaskData.TaskId,
            state.RequestedBy,
            state.ProjectData
        );
    }
}
