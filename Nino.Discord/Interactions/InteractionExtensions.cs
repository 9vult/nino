// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions;

public static class InteractionExtensions
{
    public static async Task<RuntimeResult> FailAsync(
        this IDiscordInteraction interaction,
        string message
    )
    {
        var embed = new EmbedBuilder()
            .WithTitle("Baka.")
            .WithDescription(message)
            .WithColor(0xd797ff)
            .Build();
        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Failure;
    }
}
