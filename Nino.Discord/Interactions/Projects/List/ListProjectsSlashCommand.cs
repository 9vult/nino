// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord.Interactions;
using Nino.Core.Features.Queries.Projects.List;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    [SlashCommand("list", "List all projects")]
    public async Task<RuntimeResult> ListAsync()
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var request = await listProjectsHandler.HandleAsync(
            new ListProjectsQuery(groupId, requestedBy)
        );

        if (!request.IsSuccess)
            return await interaction.FailAsync(request.Status, locale);

        var projects = request.Value;

        if (projects.Count == 0)
        {
            await interaction.FollowupAsync(T("project.list.empty", locale));
            return ExecutionResult.Success;
        }

        var nick = T("project.list.nickname", locale);
        var owner = T("project.list.owner", locale);
        var isPrivate = T("project.list.isPrivate", locale);
        var isArchived = T("project.list.isArchived", locale);
        var episodeCount = T("project.list.episodeCount", locale);
        var observerCount = T("project.list.observerCount", locale);
        var hasDelegateObserver = T("project.list.hasDelegateObserver", locale);
        var airNotificationsEnabled = T("project.list.airNotificationsEnabled", locale);
        var congaRemindersEnabled = T("project.list.congaRemindersEnabled", locale);
        var taskCompletionStatus = T("project.list.taskCompletionStatus", locale);

        var yes = T("project.list.true", locale);
        var no = T("project.list.false", locale);

        var tableData = new Table
        {
            Columns =
            [
                new
                {
                    title = nick,
                    dataIndex = nameof(ListProjectsResult.Nickname),
                    width = 200,
                },
                new
                {
                    title = owner,
                    dataIndex = nameof(ListProjectsResult.OwnerName),
                    width = 200,
                },
                new { title = isPrivate, dataIndex = nameof(ListProjectsResult.IsPrivate) },
                new { title = isArchived, dataIndex = nameof(ListProjectsResult.IsArchived) },
                new { title = episodeCount, dataIndex = nameof(ListProjectsResult.EpisodeCount) },
                new { title = observerCount, dataIndex = nameof(ListProjectsResult.ObserverCount) },
                new
                {
                    title = hasDelegateObserver,
                    dataIndex = nameof(ListProjectsResult.HasDelegateObserver),
                    width = 150,
                },
                new
                {
                    title = airNotificationsEnabled,
                    dataIndex = nameof(ListProjectsResult.AirNotificationsEnabled),
                    width = 150,
                },
                new
                {
                    title = congaRemindersEnabled,
                    dataIndex = nameof(ListProjectsResult.CongaRemindersEnabled),
                    width = 150,
                },
                new
                {
                    title = taskCompletionStatus,
                    dataIndex = "TaskCompletion",
                    width = 200,
                },
            ],

            DataSource = projects
                .Select(p => new
                {
                    p.Nickname,
                    p.OwnerName,
                    EpisodeCount = $"{p.EpisodeCount}",
                    ObserverCount = $"{p.ObserverCount}",
                    IsPrivate = p.IsPrivate ? yes : no,
                    IsArchived = p.IsArchived ? yes : no,
                    AirNotificationsEnabled = p.AirNotificationsEnabled ? yes : no,
                    CongaRemindersEnabled = p.CongaRemindersEnabled ? yes : no,
                    HasDelegateObserver = p.HasDelegateObserver ? yes : no,
                    TaskCompletion = $"{p.TotalNonPseudoTaskCompletedCount}/{p.TotalNonPseudoTaskCount} ({p.TotalTaskCompletedCount}/{p.TotalTaskCount})*",
                })
                .ToList<object>(),
        };

        var response = await httpClient.PostAsync(
            "https://api.quickchart.io/v1/table",
            new StringContent(
                JsonSerializer.Serialize(new { data = tableData }),
                Encoding.UTF8,
                "application/json"
            )
        );

        if (response.IsSuccessStatusCode)
        {
            using var stream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
            // successEmbed = successEmbed.WithImageUrl("attachment://congo.png");
            await interaction.FollowupWithFileAsync(
                stream,
                "projects.png" // ,
            // embed: successEmbed.Build()
            );
            return ExecutionResult.Success;
        }

        return ExecutionResult.Success;
    }

    private class Table
    {
        [JsonPropertyName("columns")]
        public required List<object> Columns { get; init; }

        [JsonPropertyName("dataSource")]
        public required List<object> DataSource { get; init; }
    }
}
