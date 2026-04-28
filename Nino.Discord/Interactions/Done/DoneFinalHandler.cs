// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.MarkDone;
using Nino.Core.Features.Queries.Episodes.GetProgressResponseData;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Tasks.GetTaskInfo;
using Nino.Discord.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule
{
    private async Task<RuntimeResult> HandleFinalAsync(
        IDiscordInteraction interaction,
        ProjectId projectId,
        EpisodeId episodeId,
        TaskId taskId,
        UserId requestedBy,
        GetGenericProjectDataResponse pData
    )
    {
        var locale = interaction.UserLocale;

        var result = await doneHandler
            .HandleAsync(new MarkTaskDoneCommand(projectId, episodeId, taskId, requestedBy))
            .ThenAsync(_ => getTaskInfoHandler.HandleAsync(new GetTaskInfoQuery(taskId)))
            .ThenAsync(
                (_, _) =>
                    getProgressResponseDataHandler.HandleAsync(
                        new GetProgressResponseDataQuery(episodeId, true)
                    )
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext
                {
                    Overrides = new Dictionary<ResultStatus, string>
                    {
                        [ResultStatus.BadRequest] = "done.badRequest",
                    },
                }
            );
        }

        var episodeIsDone = result.Value.Item1;
        var taskInfo = result.Value.Item2;
        var progressResponseInfo = result.Value.Item3;

        var body = new StringBuilder();
        body.AppendLine(T("done.success", locale, taskInfo.TaskName, taskInfo.EpisodeNumber));

        if (episodeIsDone)
            body.AppendLine(T("done.episodeComplete", locale, taskInfo.EpisodeNumber));

        if (progressResponseInfo.ProgressResponseType is ProgressResponseType.Verbose)
        {
            body.AppendLine();
            foreach (var task in progressResponseInfo.Statuses.OrderBy(t => t.Weight))
            {
                if (task.IsDone)
                    body.Append($"~~{task.Abbreviation}~~ ");
                else
                    body.Append($"**{task.Abbreviation}** ");
            }
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle($"✅ {T("done.response.title", locale)}")
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
