// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.AdditionalStaff.SetWeight;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.AdditionalStaff;

public partial class AdditionalStaffModule
{
    [SlashCommand("set-weight", "Set the weight of an Additional Staff position")]
    public async Task<RuntimeResult> SetWeightAsync(
        [MaxLength(32)] string alias,
        [MaxLength(32)] string episodeNumber,
        [MaxLength(16)] string abbreviation,
        decimal weight
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

        var result = await setWeightHandler.HandleAsync(
            new SetAdditionalStaffWeightCommand(
                projectId,
                episodeNumber,
                abbreviation,
                weight,
                requestedBy
            )
        );

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
            .WithDescription(T("additionalStaff.setWeight.success", locale, abbreviation, weight))
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
