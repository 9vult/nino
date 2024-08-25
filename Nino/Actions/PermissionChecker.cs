using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Actions
{
    internal static class PermissionChecker
    {
        /// <summary>
        /// Check if the bot has permission to use a channel
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        /// <returns>True if the bot has sufficient permissions</returns>
        public static bool CheckPermissions(ulong channelId)
        {
            var channel = Nino.Client.GetChannel(channelId);
            if (channel == null) return false;
            if (channel.GetChannelType() != ChannelType.Text) return false;

            var textChannel = (SocketTextChannel)channel;
            var perms = textChannel.Guild.GetUser(Nino.Client.CurrentUser.Id).GetPermissions(textChannel);

            return perms.ViewChannel
                && perms.SendMessages
                && perms.EmbedLinks;
        }

        /// <summary>
        /// Check if the bot has permission to use a releases channel
        /// </summary>
        /// <param name="channelId">ID of the channel</param>
        /// <returns>True if the bot has sufficient permissions</returns>
        public static bool CheckReleasePermissions(ulong channelId)
        {
            var channel = Nino.Client.GetChannel(channelId);
            if (channel == null) return false;
            if (channel.GetChannelType() != ChannelType.Text) return false;

            var textChannel = (SocketTextChannel)channel;
            var perms = textChannel.Guild.GetUser(Nino.Client.CurrentUser.Id).GetPermissions(textChannel);

            return perms.ViewChannel
                && perms.SendMessages
                && perms.EmbedLinks
                && perms.MentionEveryone
                && perms.ManageMessages;
        }
    }
}
