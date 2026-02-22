// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.Project.Delete;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [ComponentInteraction("nino.project.delete.confirm:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmDeleteAsync(Guid stateId)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var commandDto = await stateService.LoadStateAsync<DeleteProjectCommand>(stateId);
        if (commandDto is null)
            return await interaction.FailAsync(T("error.db", locale));

        // Verify button was clicked by initiator
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != commandDto.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete state, won't be needed regardless of the final status
        await stateService.DeleteStateAsync(stateId);

        var (nickname, title, type, _) = await dataService.GetProjectBasicInfoAsync(
            commandDto.ProjectId
        );
        var header = $"{title} ({type.ToFriendlyString(locale)})";

        logger.LogTrace("Project deletion confirmed by {UserId}", commandDto.RequestedBy);

        var (status, json) = await deleteHandler.HandleAsync(commandDto);

        if (status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.projectNotFound", locale),
                    _ => T("error.generic", locale),
                }
            );
        }

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

        var file = new MemoryStream();
        file.Write(Encoding.UTF8.GetBytes(json!));
        file.Position = 0;

        await interaction.FollowupWithFileAsync(
            file,
            $"{nickname}.json",
            T("project.export", locale, nickname)
        );
        return ExecutionResult.Success;
    }
}
