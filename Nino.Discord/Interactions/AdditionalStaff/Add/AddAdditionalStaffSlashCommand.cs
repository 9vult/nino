// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Features.AdditionalStaff.Add;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.AdditionalStaff;

public partial class AdditionalStaffModule
{
    [SlashCommand("add", "Add an Additional Staff to an episode")]
    public async Task<RuntimeResult> AddAsync(
        string alias,
        string episodeNumber,
        SocketUser member,
        [MaxLength(16)] string abbreviation,
        [MaxLength(32)] string fullName,
        bool isPseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

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

        var commandDto = new AddAdditionalStaffCommand(
            projectId,
            memberId,
            episodeNumber,
            abbreviation,
            fullName,
            isPseudo,
            requestedBy
        );

        var result = await addHandler.HandleAsync(commandDto);

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.episodeNotFound", locale),
                    ResultStatus.Conflict => T(
                        "additionalStaff.creation.conflict",
                        locale,
                        abbreviation
                    ),
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
            .WithDescription(
                T(
                    "additionalStaff.creation.success",
                    locale,
                    staffMention,
                    abbreviation,
                    episodeNumber
                )
            )
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
