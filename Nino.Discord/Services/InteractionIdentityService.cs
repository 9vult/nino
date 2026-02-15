// SPDX-License-Identifier: MPL-2.0

using Discord;
using Nino.Core.Services;
using Nino.Discord.Entities;

namespace Nino.Discord.Services;

public class InteractionIdentityService(IIdentityService identityService)
    : IInteractionIdentityService
{
    /// <inheritdoc />
    public async Task<InteractionIdentityResult> GetUserAndGroupAsync(
        IDiscordInteraction interaction
    )
    {
        var userId = await identityService.GetOrCreateUserByDiscordIdAsync(
            interaction.User.Id,
            interaction.User.Username
        );
        var groupId = await identityService.GetOrCreateGroupByDiscordIdAsync(
            interaction.GuildId!.Value
        );

        return new InteractionIdentityResult(userId, groupId);
    }
}
