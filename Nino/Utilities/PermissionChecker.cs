using Discord;
using Discord.WebSocket;
using Fergun.Interactive;
using Nino.Records;
using Nino.Records.Enums;
using static Localizer.Localizer;

namespace Nino.Utilities
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
            if (channel.GetChannelType() != ChannelType.Text && channel.GetChannelType() != ChannelType.News) return false;

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
            if (channel.GetChannelType() != ChannelType.Text && channel.GetChannelType() != ChannelType.News) return false;

            var textChannel = (SocketTextChannel)channel;
            var perms = textChannel.Guild.GetUser(Nino.Client.CurrentUser.Id).GetPermissions(textChannel);

            return perms.ViewChannel
                && perms.SendMessages
                && perms.EmbedLinks
                && perms.MentionEveryone
                && perms.ManageMessages;
        }

        // Check if the bot has permission to send progress updates
        public static async Task<bool> Precheck(InteractiveService service, SocketInteraction interaction, Project project, string lng, bool isRelease = false, bool isConga = false)
        {
            // Check permissions
            if (!isConga && isRelease && CheckReleasePermissions(project.ReleaseChannelId))
                    return true;
            else if (!isConga && !isRelease && CheckPermissions(project.UpdateChannelId))
                return true;
            else if (isConga && CheckPermissions(interaction.Channel.Id))
                return true;
            

            // No permissions... do the thing

            var channelMention = isRelease 
                ? $"<#{project.ReleaseChannelId}>" 
                : !isConga 
                    ? $"<#{project.UpdateChannelId}>" 
                    : $"<#{interaction.Channel.Id}>";
            var questionBody = !isConga 
                ? T("missingPermsPrecheck.question", lng, channelMention)
                : T("missingPermsPrecheck.question.conga", lng, channelMention);

            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var component = new ComponentBuilder()
                .WithButton(T("missingPermsPrecheck.no.button", lng), "ninoprecheckcancel", ButtonStyle.Danger)
                .WithButton(T("missingPermsPrecheck.yes.button", lng), "ninoprecheckproceed", ButtonStyle.Secondary)
                .Build();
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(questionBody)
                .WithCurrentTimestamp()
                .Build();

            var questionResponse = await interaction.ModifyOriginalResponseAsync(m => {
                m.Embed = questionEmbed;
                m.Components = component;
            });

            // Wait for response
            var questionResult = await service.NextMessageComponentAsync(
                m => m.Message.Id == questionResponse.Id, timeout: TimeSpan.FromSeconds(60));

            bool fullSend = false;
            string finalBody = string.Empty;

            if (!questionResult.IsSuccess)
                finalBody = T("progress.done.inTheDust.timeout", lng);
            else
            {
                if (questionResult.Value.Data.CustomId == "ninoprecheckcancel")
                    finalBody = T("missingPermsPrecheck.no.response", lng);
                else
                {
                    fullSend = true;
                    finalBody = T("missingPermsPrecheck.yes.response", lng);
                }
            }

            // Update the question embed to replect the choice
            var editedEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(finalBody)
                .WithCurrentTimestamp()
                .Build();

            await questionResponse.ModifyAsync(m => {
                m.Components = null;
                m.Embed = editedEmbed;
            });

            return fullSend;
        }
    }
}
