// SPDX-License-Identifier: MPL-2.0

using Discord;

namespace Nino.Discord.Services;

public interface IBotPermissionsService
{
    /// <summary>
    /// Check if the bot has permission to send messages to a channel
    /// </summary>
    /// <param name="channelId">Discord channel ID</param>
    /// <returns><see langword="true"/> if the bot can send messages to the channel</returns>
    bool HasMessagePermissions(ulong channelId);

    /// <summary>
    /// Check if the bot has permission to publish messages to a channel
    /// </summary>
    /// <param name="channelId">Discord channel ID</param>
    /// <returns><see langword="true"/> if the bot can publish messages to the channel</returns>
    bool HasReleasePermissions(ulong channelId);

    /// <summary>
    /// Get the permissions the bot has for a channel
    /// </summary>
    /// <param name="channelId">Discord channel ID</param>
    /// <returns>Permissions the bot has</returns>
    ChannelPermissions? GetChannelPermissions(ulong channelId);
}
