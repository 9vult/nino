// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions;

public static class InteractionExtensions
{
    extension(IDiscordInteraction interaction)
    {
        public async Task<RuntimeResult> FailAsync(string message, bool ephemeral = false)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed, ephemeral: ephemeral);
            return ExecutionResult.Failure;
        }

        public async Task<RuntimeResult> FailAsync(
            string localizationKey,
            string locale,
            Dictionary<string, object> localizationArgs,
            bool ephemeral = false
        )
        {
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(T(localizationKey, locale, localizationArgs))
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed, ephemeral: ephemeral);
            return ExecutionResult.Failure;
        }
    }
}
