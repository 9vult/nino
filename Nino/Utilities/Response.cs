using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Nino.Utilities
{
    internal static class Response
    {
        public static async Task<RuntimeResult> Fail(string message, SocketInteraction interaction)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed);
            return ExecutionResult.Failure;
        }

        public static async Task<RuntimeResult> Info(string message, SocketInteraction interaction)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Info.")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed);
            return ExecutionResult.Success;
        }
        
        public static async Task<RuntimeResult> DbError(string message, SocketInteraction interaction)
        {
            var embed = new EmbedBuilder()
                .WithTitle("痛い！ Database error")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed);
            return ExecutionResult.Success;
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
