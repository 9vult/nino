// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.PinchHitter.Set;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    public partial class PinchHitterModule
    {
        [SlashCommand("set", "Set a pinch hitter for an episode")]
        public async Task<RuntimeResult> SetPinchHitterAsync(
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
            var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(
                interaction
            );
            var (resolveStatus, projectId) = await projectResolver.HandleAsync(
                new ResolveProjectQuery(alias, groupId, requestedBy)
            );

            if (resolveStatus is not ResultStatus.Success)
                return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

            var memberId = await identityService.GetOrCreateUserByDiscordIdAsync(
                member.Id,
                member.Username
            );

            var result = await setPinchHitterHandler.HandleAsync(
                new SetPinchHitterCommand(
                    projectId,
                    episodeNumber,
                    abbreviation,
                    memberId,
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
                        "keyStaff.pinchHitter.creation.success",
                        locale,
                        staffMention,
                        episodeNumber,
                        abbreviation
                    )
                )
                .Build();

            await interaction.FollowupAsync(embed: embed);
            return ExecutionResult.Success;
        }
    }
}
