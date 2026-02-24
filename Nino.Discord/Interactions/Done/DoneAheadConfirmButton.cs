// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Dtos;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    [ComponentInteraction("nino.done.ahead.confirm:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmAheadAsync(Guid stateId)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var state = await stateService.LoadStateAsync<DoneStateDto>(stateId);
        if (state is null)
            return await interaction.FailAsync(T("error.db", locale));

        // Verify button was clicked by initiator
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != state.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        logger.LogTrace("/done with ahead status confirmed by user {UserId}", state.RequestedBy);

        // Check if the episode has aired
        if (!await dataService.GetHasEpisodeAiredAsync(state.ProjectId, state.TaskEpisodeNumber))
        {
            // The episode hasn't aired, get confirmation (keep state)
            return await SendUnairedEmbedAsync(interaction, state);
        }

        var data = await dataService.GetProjectBasicInfoAsync(state.ProjectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var difference = await dataService.GetEpisodeDifferenceAsync(
            state.ProjectId,
            state.WorkingEpisodeNumber,
            state.TaskEpisodeNumber
        );

        var dict = new Dictionary<string, object>
        {
            ["number"] = difference,
            ["taskName"] = state.TaskName,
        };

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("action.question", locale))
            .WithDescription(T("task.complete.ahead.response", locale, dict))
            .WithCurrentTimestamp()
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = null;
        });

        // Delete state and go to finale
        await stateService.DeleteStateAsync(stateId);
        return await HandleFinalAsync(interaction, state);
    }
}
