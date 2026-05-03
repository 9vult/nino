// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Tasks.GetTaskInfo;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    private async Task<RuntimeResult> SendUnairedEmbedAsync(
        IDiscordInteraction interaction,
        UserId requestedBy,
        GetGenericProjectDataResponse pData,
        GetTaskInfoResponse tData
    )
    {
        var locale = interaction.UserLocale;
        var state = new DoneState(pData, tData, 0, requestedBy);
        var stateId = await stateService.SaveStateAsync(state);

        var questionEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("action.question", locale))
            .WithDescription(T("done.unaired.question", locale, tData.EpisodeNumber))
            .Build();

        var cancelId = $"nino.done.cancel:{stateId}";
        var confirmId = $"nino.done.unaired.confirm:{stateId}";

        var component = new ComponentBuilder()
            .WithButton(T("button.no", locale), cancelId, ButtonStyle.Secondary)
            .WithButton(T("button.doIt", locale), confirmId, ButtonStyle.Secondary)
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = questionEmbed;
            m.Components = component;
        });
        return ExecutionResult.Success;
    }
}
