// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;

namespace Nino.Discord.Services;

public class BotPermissionsService(DiscordSocketClient client) : IBotPermissionsService
{
    /// <inheritdoc />
    public bool HasMessagePermissions(ulong channelId)
    {
        var channel = client.GetChannel(channelId) as SocketTextChannel;

        if (
            channel?.ChannelType
            is not (
                ChannelType.Text
                or ChannelType.News
                or ChannelType.PrivateThread
                or ChannelType.PublicThread
            )
        )
            return false;

        var perms = channel.Guild.GetUser(client.CurrentUser.Id).GetPermissions(channel);
        return perms is { ViewChannel: true, SendMessages: true, EmbedLinks: true };
    }

    /// <inheritdoc />
    public bool HasReleasePermissions(ulong channelId)
    {
        var channel = client.GetChannel(channelId) as SocketTextChannel;

        if (channel?.ChannelType is not (ChannelType.Text or ChannelType.News))
            return false;

        var perms = channel.Guild.GetUser(client.CurrentUser.Id).GetPermissions(channel);
        return perms
            is { ViewChannel: true, SendMessages: true, EmbedLinks: true, MentionEveryone: true };
    }
}
