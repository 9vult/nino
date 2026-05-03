// SPDX-License-Identifier: MPL-2.0

using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Services;

namespace Nino.Discord.Handlers;

public sealed class PartialGroupCreatedEventHandler(
    DiscordSocketClient client,
    IIdentityService identityService,
    ILogger<PartialGroupCreatedEventHandler> logger
) : IEventHandler<PartialGroupCreatedFromDiscordEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(PartialGroupCreatedFromDiscordEvent @event)
    {
        var (groupId, discordId) = @event;

        var guild = client.GetGuild(discordId);
        if (guild is null)
        {
            logger.LogWarning(
                "No guild {DiscordId} found to enrich group {GroupId}",
                discordId,
                groupId
            );
            return;
        }

        logger.LogInformation(
            "Enriching group {GroupId} ({DiscordId}) with name {Name}",
            groupId,
            discordId,
            guild.Name
        );

        if (!await identityService.UpdateGroupNameAsync(groupId, guild.Name))
            logger.LogWarning(
                "Failed to update group {GroupId}'s name because it does not exist!",
                groupId
            );
    }
}
