// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.AdditionalStaff.Remove;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.AdditionalStaff;

public partial class AdditionalStaffModule
{
    [SlashCommand("add", "Remove an Additional Staff from an episode")]
    public async Task<RuntimeResult> RemoveAsync(
        string alias,
        string episodeNumber,
        [MaxLength(16)] string abbreviation
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        episodeNumber = episodeNumber.Trim();
        abbreviation = abbreviation.Trim();

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var commandDto = new RemoveAdditionalStaffCommand(
            projectId,
            episodeNumber,
            abbreviation,
            requestedBy
        );

        var result = await removeHandler.HandleAsync(commandDto);

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T(
                        "additionalStaff.notFound",
                        locale,
                        abbreviation,
                        episodeNumber
                    ),
                    _ => T("error.generic", locale),
                }
            );
        }

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T("additionalStaff.delete.success", locale, abbreviation, episodeNumber)
            )
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
