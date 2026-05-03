// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord.Interactions;
using Nino.Core.Features.Queries.Observers.List;
using Nino.Core.Features.Queries.Projects.List;

namespace Nino.Discord.Interactions.Observers;

public partial class ObserverModule
{
    [SlashCommand("list", "List all observers")]
    public async Task<RuntimeResult> ListAsync()
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var request = await listObserversHandler.HandleAsync(
            new ListObserversQuery(groupId, requestedBy)
        );

        if (!request.IsSuccess)
            return await interaction.FailAsync(request.Status, locale);

        var observers = request.Value;

        if (observers.Count == 0)
        {
            await interaction.FollowupAsync(T("observer.list.empty", locale));
            return ExecutionResult.Success;
        }

        var group = T("observer.list.groupName", locale);
        var nick = T("observer.list.nickname", locale);
        var owner = T("observer.list.owner", locale);
        var isDelegateObserver = T("observer.list.isDelegateObserver", locale);

        var yes = T("project.list.true", locale);
        var no = T("project.list.false", locale);

        var tableData = new Table
        {
            Columns =
            [
                new
                {
                    title = group,
                    dataIndex = nameof(ListObserversResult.GroupName),
                    width = 200,
                },
                new
                {
                    title = nick,
                    dataIndex = nameof(ListObserversResult.ProjectNickname),
                    width = 200,
                },
                new
                {
                    title = owner,
                    dataIndex = nameof(ListObserversResult.OwnerName),
                    width = 200,
                },
                new
                {
                    title = isDelegateObserver,
                    dataIndex = nameof(ListObserversResult.IsDelegate),
                    width = 150,
                },
            ],

            DataSource = observers
                .Select(p => new
                {
                    p.GroupName,
                    p.ProjectNickname,
                    p.OwnerName,
                    IsDelegate = p.IsDelegate ? yes : no,
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
            await interaction.FollowupWithFileAsync(stream, "observers.png");
            return ExecutionResult.Success;
        }

        await interaction.FollowupAsync($"{response.StatusCode} {response.ReasonPhrase}");
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
