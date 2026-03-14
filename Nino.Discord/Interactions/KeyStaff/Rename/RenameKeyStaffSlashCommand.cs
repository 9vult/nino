// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.KeyStaff.Rename;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Staff.Resolve;
using Nino.Domain;
using Nino.Localization;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [SlashCommand("rename", "Rename a Key Staff")]
    public async Task<RuntimeResult> RenameAsync(
        [MaxLength(Length.Alias)] string alias,
        [MaxLength(Length.Abbreviation)] string abbreviation,
        [MaxLength(Length.Abbreviation)] string newAbbreviation,
        [MaxLength(Length.RoleName)] string newName
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        newAbbreviation = newAbbreviation.Trim().ToUpperInvariant();
        newName = newName.Trim().ToUpperInvariant();

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .ResolveAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId =>
                staffResolver.ResolveAsync(new ResolveKeyStaffQuery(prjId, abbreviation))
            );

        if (!resolve.IsSuccess)
        {
            Dictionary<string, object> errorParams = new()
            {
                ["alias"] = alias,
                ["abbreviation"] = abbreviation,
            };
            return await interaction.FailAsync(
                resolve.Status switch
                {
                    ResultStatus.ProjectNotFound => "project.resolution.failed",
                    ResultStatus.StaffNotFound => "keyStaff.resolution.failed",
                    _ => "error.generic",
                },
                locale,
                errorParams
            );
        }

        var (projectId, staffId) = resolve.Value;

        var command = new RenameKeyStaffCommand(
            ProjectId: projectId,
            StaffId: staffId,
            RequestedBy: requestedBy,
            NewAbbreviation: newAbbreviation,
            NewName: newName
        );

        var result = await renameHandler.HandleAsync(command);

        if (!result.IsSuccess)
        {
            Dictionary<string, object> errorParams = new() { ["abbreviation"] = abbreviation };
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => "error.permissions",
                    ResultStatus.Conflict => "keyStaff.creation.conflict",
                    _ => "error.generic",
                },
                locale,
                errorParams
            );
        }

        var (projectTitle, projectType, _) = result.Value;
        var header = $"{projectTitle} ({projectType.ToFriendlyString(locale)})";

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T("keyStaff.rename.success", locale, abbreviation, newAbbreviation, newName)
            )
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
