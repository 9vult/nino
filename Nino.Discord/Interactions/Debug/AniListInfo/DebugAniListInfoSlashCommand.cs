// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord.Interactions;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Debug;

public partial class DebugModule
{
    [SlashCommand("anilist-info", "AniList information")]
    public async Task<RuntimeResult> GetAniListInfoAsync(
        [Summary("anilistId"), MinValue(1)] int rawAniListId
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var aniListId = AniListId.From(rawAniListId);

        var request = await aniListService.GetAnimeAsync(aniListId);
        if (!request.IsSuccess)
            return await interaction.FailAsync(request.Status, locale);

        var json = JsonSerializer.Serialize(request.Value, JsonOptions);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await interaction.FollowupWithFileAsync(stream, $"{aniListId}.json");

        return ExecutionResult.Success;
    }
}
