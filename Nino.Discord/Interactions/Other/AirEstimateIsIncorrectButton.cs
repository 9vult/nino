// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Events;
using Nino.Core.Features.Commands.Episodes.RejectAirEstimate;
using Nino.Core.Features.Queries.Episodes.GetAirNotificationData;
using Nino.Core.Services;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.Other;

public class AirEstimateIsIncorrectButton(
    IStateService stateService,
    IIdentityService identityService,
    RejectAirEstimateHandler rejectHandler,
    GetAirNotificationDataHandler getDataHandler,
    ILogger<AirEstimateIsIncorrectButton> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("nino.air.estimate.incorrect:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> RejectAirTimeAsync(string rawId)
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
            return await interaction.FailAsync(T("error.state", locale));

        var result = await rejectHandler.HandleAsync(
            new RejectAirEstimateCommand(@event.EpisodeId, requestedBy)
        );

        if (!result.IsSuccess)
            return await interaction.FailAsync(result.Status, locale);

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
            return await interaction.FailAsync(queryResult.Status, locale);
        }

        var data = queryResult.Value;
        locale = data.Locale.ToDiscordLocale();
        var absoluteTime = $"<t:{@event.AirTime.ToUnixTimeSeconds()}:D>";
        var relativeTime = $"<t:{@event.AirTime.ToUnixTimeSeconds()}:R>";

        var body = new StringBuilder();
        body.AppendLine(T("episode.aired.body", locale, absoluteTime, relativeTime));
        body.AppendLine(T("episode.aired.estimate.response.incorrect", locale));

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
