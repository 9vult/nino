// SPDX-License-Identifier: MPL-2.0

using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Services;

namespace Nino.Discord.Handlers;

public sealed class PartialUserCreatedEventHandler(
    DiscordSocketClient client,
    IIdentityService identityService,
    ILogger<PartialUserCreatedEventHandler> logger
) : IEventHandler<PartialUserCreatedFromDiscordEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(PartialUserCreatedFromDiscordEvent @event)
    {
        var (userId, discordId) = @event;

        var user = await client.GetUserAsync(discordId);
        if (user is null)
        {
            logger.LogWarning(
                "No user {DiscordId} found to enrich user {UserId}",
                discordId,
                userId
            );
            return;
        }

        logger.LogInformation(
            "Enriching user {UserId} ({DiscordId}) with name {Name}",
            userId,
            discordId,
            user.Username
        );

        if (!await identityService.UpdateUserNameAsync(userId, user.Username))
            logger.LogWarning(
                "Failed to update user {UserId}'s name because it does not exist!",
                userId
            );
    }
}
