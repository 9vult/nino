// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [ComponentInteraction("nino:project:delete:confirm:*:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmDeleteAsync(Guid projectId, Guid userId)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var (status, json) = await deleteHandler.HandleAsync(
            new ProjectDeleteAction(projectId, userId)
        );

        if (status is ResultStatus.Success)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle(T("project.delete.title", locale))
                .WithDescription(T("project.delete.complete", locale))
                .WithCurrentTimestamp()
                .Build();

            await interaction.ModifyOriginalResponseAsync(m =>
            {
                m.Embed = embed;
                m.Components = null;
            });
        }

        return ExecutionResult.Success;
    }
}
