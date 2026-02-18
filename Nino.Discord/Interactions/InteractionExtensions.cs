// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions;

public static class InteractionExtensions
{
    public static async Task<RuntimeResult> FailAsync(
        this IDiscordInteraction interaction,
        string message,
        bool ephemeral = false
    )
    {
        var embed = new EmbedBuilder()
            .WithTitle("Baka.")
            .WithDescription(message)
            .WithColor(0xd797ff)
            .Build();
        await interaction.FollowupAsync(embed: embed, ephemeral: ephemeral);
        return ExecutionResult.Failure;
    }
}
