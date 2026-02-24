// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Nino.Core.Dtos;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    private async Task<ExecutionResult> SendAheadEmbedAsync(
        SocketInteraction interaction,
        DoneStateDto commandDto,
        string workingEpisode
    )
    {
        var (projectId, _, taskEpisodeNumber, _, taskName, _) = commandDto;
        var locale = interaction.UserLocale;

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        // Save the command state
        var stateId = await stateService.SaveStateAsync(commandDto);

        var questionEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("action.question", locale))
            .WithDescription(
                T(
                    "task.complete.ahead.question",
                    locale,
                    workingEpisode,
                    taskName,
                    taskEpisodeNumber
                )
            )
            .WithCurrentTimestamp()
            .Build();

        var noId = $"nino.done.ahead.cancel:{stateId}";
        var yesId = $"nino.done.ahead.confirm:{stateId}";

        var component = new ComponentBuilder()
            .WithButton(T("button.no", locale), noId, ButtonStyle.Secondary)
            .WithButton(T("button.doIt", locale), yesId, ButtonStyle.Secondary)
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = questionEmbed;
            m.Components = component;
        });

        return ExecutionResult.Success;
    }
}
