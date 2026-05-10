// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Events;
using Nino.Core.Features.Commands.Episodes.ConfirmAirEstimate;
using Nino.Core.Features.Queries.Episodes.GetAirNotificationData;
using Nino.Core.Services;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.Other;

public class AirEstimateIsCorrectButton(
    IStateService stateService,
    IIdentityService identityService,
    ConfirmAirEstimateHandler confirmHandler,
    GetAirNotificationDataHandler getDataHandler,
    ILogger<AirEstimateIsCorrectButton> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("nino.air.estimate.correct:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmAirTimeAsync(string rawId)
    {
        if (!StateId.TryParse(rawId, out var stateId))
        {
            logger.LogError("Could not parse state id: {StateId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var requestedBy = await identityService.GetOrCreateUserByDiscordIdAsync(
            interaction.User.Id,
            interaction.User.Username
        );

        var @event = await stateService.LoadStateAsync<EpisodeAiredEstimateEvent>(stateId);
        if (@event is null)
            return await interaction.FailAsync(T("error.state", locale), ephemeral: true);

        var result = await confirmHandler.HandleAsync(
            new ConfirmAirEstimateCommand(@event.EpisodeId, requestedBy)
        );

        if (!result.IsSuccess)
            return await interaction.FailAsync(result.Status, locale, ephemeral: true);

        await stateService.DeleteStateAsync(stateId);

        var queryResult = await getDataHandler.HandleAsync(
            new GetAirNotificationDataQuery(@event.EpisodeId)
        );
        if (!queryResult.IsSuccess)
        {
            logger.LogWarning(
                "Failed to get air notification data for episode {EpisodeId}",
                @event.EpisodeId
            );
            return await interaction.FailAsync(queryResult.Status, locale, ephemeral: true);
        }

        var data = queryResult.Value;
        locale = data.Locale.ToDiscordLocale();
        var absoluteTime = $"<t:{@event.AirTime.ToUnixTimeSeconds()}:D>";
        var relativeTime = $"<t:{@event.AirTime.ToUnixTimeSeconds()}:R>";

        var body = new StringBuilder();
        body.AppendLine(T("episode.aired.body", locale, absoluteTime, relativeTime));

        var embed = new EmbedBuilder()
            .WithProjectInfo(data.ProjectData, locale)
            .WithTitle(T("episode.aired.estimate.title", locale, data.EpisodeNumber))
            .WithDescription(body.ToString())
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = null;
        });
        return ExecutionResult.Success;
    }
}
