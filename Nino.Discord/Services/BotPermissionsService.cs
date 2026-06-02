// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Nino.Discord.Entities;

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

        var guildUser = channel.Guild.GetUser(client.CurrentUser.Id);
        var guildPerms = guildUser.GuildPermissions;
        var channelPerms = guildUser.GetPermissions(channel);

        if (guildPerms.Administrator)
            return true;

        return (guildPerms.ViewChannel || channelPerms.ViewChannel)
            && (guildPerms.SendMessages || channelPerms.SendMessages)
            && (guildPerms.EmbedLinks || channelPerms.EmbedLinks);
    }

    /// <inheritdoc />
    public bool HasReleasePermissions(ulong channelId)
    {
        var channel = client.GetChannel(channelId) as SocketTextChannel;

        if (channel?.ChannelType is not (ChannelType.Text or ChannelType.News))
            return false;

        var guildUser = channel.Guild.GetUser(client.CurrentUser.Id);
        var guildPerms = guildUser.GuildPermissions;
        var channelPerms = guildUser.GetPermissions(channel);

        if (guildPerms.Administrator)
            return true;

        return (guildPerms.ViewChannel || channelPerms.ViewChannel)
            && (guildPerms.SendMessages || channelPerms.SendMessages)
            && (guildPerms.EmbedLinks || channelPerms.EmbedLinks)
            && (guildPerms.MentionEveryone || channelPerms.MentionEveryone);
    }

    /// <inheritdoc />
    public BotPermissions? GetChannelPermissions(ulong channelId)
    {
        var channel = client.GetChannel(channelId) as SocketTextChannel;
        if (channel?.ChannelType is not (ChannelType.Text or ChannelType.News))
            return null;

        var guildUser = channel.Guild.GetUser(client.CurrentUser.Id);
        var guildPerms = guildUser.GuildPermissions;
        var channelPerms = guildUser.GetPermissions(channel);

        if (guildPerms.Administrator)
        {
            return new BotPermissions(
                ViewChannel: true,
                SendMessages: true,
                EmbedLinks: true,
                MentionEveryone: true
            );
        }

        return new BotPermissions(
            ViewChannel: guildPerms.ViewChannel || channelPerms.ViewChannel,
            SendMessages: guildPerms.SendMessages || channelPerms.SendMessages,
            EmbedLinks: guildPerms.EmbedLinks || channelPerms.EmbedLinks,
            MentionEveryone: guildPerms.MentionEveryone || channelPerms.MentionEveryone
        );
    }
}
