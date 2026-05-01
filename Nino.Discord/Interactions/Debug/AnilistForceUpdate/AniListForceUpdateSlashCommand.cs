// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Nino.Core.Features;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Debug;

public partial class DebugModule
{
    [SlashCommand("anilist-force-update", "Force update an AniList entry")]
    public async Task<RuntimeResult> ForceUpdateAniListAsync(
        [Summary("anilistId"), MinValue(1)] int rawAniListId
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (userId, _) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var isAdmin = await verificationService.VerifySystemAdministratorAsync(userId);

        if (!isAdmin)
            return await interaction.FailAsync(ResultStatus.Unauthorized, locale);

        var aniListId = AniListId.From(rawAniListId);

        var request = await aniListService.GetAnimeAsync(aniListId);
        if (!request.IsSuccess)
            return await interaction.FailAsync(request.Status, locale);

        await interaction.FollowupAsync(T("action.completed", locale));

        return ExecutionResult.Success;
    }
}
