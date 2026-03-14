// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.AdditionalStaff.Rename;
using Nino.Core.Features.Queries.Episode.Resolve;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Staff.Resolve;
using Nino.Domain;
using Nino.Localization;

namespace Nino.Discord.Interactions.AdditionalStaff;

public partial class AdditionalStaffModule
{
    [SlashCommand("rename", "Rename an Additional Staff")]
    public async Task<RuntimeResult> RenameAsync(
        [MaxLength(Length.Alias)] string alias,
        [MaxLength(Length.EpisodeNumber)] string episodeNumber,
        [MaxLength(Length.Abbreviation)] string abbreviation,
        [MaxLength(Length.Abbreviation)] string newAbbreviation,
        [MaxLength(Length.RoleName)] string newName
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        episodeNumber = episodeNumber.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        newAbbreviation = newAbbreviation.Trim().ToUpperInvariant();
        newName = newName.Trim().ToUpperInvariant();

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .ResolveAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId =>
                episodeResolver.ResolveAsync(new ResolveEpisodeQuery(prjId, episodeNumber))
            )
            .ThenAsync(
                (_, epId) =>
                    staffResolver.ResolveAsync(new ResolveAdditionalStaffQuery(epId, abbreviation))
            );

        if (!resolve.IsSuccess)
        {
            Dictionary<string, object> errorParams = new()
            {
                ["alias"] = alias,
                ["episode"] = episodeNumber,
                ["abbreviation"] = abbreviation,
            };
            return await interaction.FailAsync(
                resolve.Status switch
                {
                    ResultStatus.ProjectNotFound => "project.resolution.failed",
                    ResultStatus.EpisodeNotFound => "episode.resolution.failed",
                    ResultStatus.StaffNotFound => "additionalStaff.resolution.failed",
                    _ => "error.generic",
                },
                locale,
                errorParams
            );
        }

        var (projectId, episodeId, staffId) = resolve.Value;

        var command = new RenameAdditionalStaffCommand(
            ProjectId: projectId,
            EpisodeId: episodeId,
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
                    ResultStatus.Conflict => "additionalStaff.creation.conflict",
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
                T("additionalStaff.rename.success", locale, abbreviation, newAbbreviation, newName)
            )
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
