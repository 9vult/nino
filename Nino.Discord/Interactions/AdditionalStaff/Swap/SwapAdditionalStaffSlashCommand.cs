// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Features.AdditionalStaff.Swap;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.AdditionalStaff;

public partial class AdditionalStaffModule
{
    [SlashCommand("swap", "Swap an Additional Staff into the episode")]
    public async Task<RuntimeResult> SwapAsync(
        [MaxLength(32)] string alias,
        [MaxLength(32)] string episodeNumber,
        [MaxLength(16)] string abbreviation,
        SocketUser member
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

        var memberId = await identityService.GetOrCreateUserByDiscordIdAsync(
            member.Id,
            member.Username
        );

        var result = await swapHandler.HandleAsync(
            new SwapAdditionalStaffCommand(
                projectId,
                episodeNumber,
                memberId,
                abbreviation,
                requestedBy
            )
        );

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("additionalStaff.notFound", locale),
                    _ => T("error.generic", locale),
                }
            );
        }

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var staffMention = $"<@{member.Id}>";
        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("additionalStaff.swap.success", locale, staffMention, abbreviation))
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
