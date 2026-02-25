// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Dtos;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    [ComponentInteraction("nino.done.unaired.confirm:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmUnairedAsync(string id)
    {
        var stateId = Guid.Parse(id);
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

        logger.LogTrace("/done with unaired status confirmed by user {UserId}", state.RequestedBy);

        var data = await dataService.GetProjectBasicInfoAsync(state.ProjectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var body = new StringBuilder();
        body.AppendLine(T("task.complete.unaired.response", locale));

        // Check if we need to add the ahead response embed
        if (state.WorkingEpisodeNumber != state.TaskEpisodeNumber)
        {
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
            body.AppendLine(T("task.complete.ahead.response", locale, dict));
        }

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("action.question", locale))
            .WithDescription(body.ToString())
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
