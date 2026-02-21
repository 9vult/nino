// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.Rename;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [SlashCommand("rename", "Swap a Key Staff into the whole project")]
    public async Task<RuntimeResult> RenameAsync(
        string alias,
        [MaxLength(16)] string abbreviation,
        [MaxLength(16)] string newAbbreviation,
        [MaxLength(32)] string fullName
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        abbreviation = abbreviation.Trim();
        fullName = fullName.Trim();

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var result = await renameHandler.HandleAsync(
            new RenameKeyStaffCommand(
                projectId,
                abbreviation,
                newAbbreviation,
                fullName,
                requestedBy
            )
        );

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("keyStaff.notFound", locale),
                    ResultStatus.Conflict => T("keyStaff.creation.conflict", locale, abbreviation),
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
                T("keyStaff.rename.success", locale, abbreviation, newAbbreviation, fullName)
            )
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
