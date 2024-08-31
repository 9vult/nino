using Discord;
using Discord.WebSocket;

namespace Nino.Utilities
{
    internal static class Response
    {
        public static async Task<bool> Fail(string message, SocketInteraction interaction)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed);
            return false;
        }

        public static async Task<bool> Info(string message, SocketInteraction interaction)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Info.")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed);
            return true;
        }

        public static AllowedMentions GenerateAllowedMentions(List<ulong> users, List<ulong> roles)
        {
            return new AllowedMentions
            {
                UserIds = users,
                RoleIds = roles
            };
        }
    }
}
