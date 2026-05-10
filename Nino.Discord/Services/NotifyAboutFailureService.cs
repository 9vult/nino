// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Nino.Core.Events;
using Nino.Core.Features.Queries.Observers.GetGenericObserverData;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Services;
using Nino.Discord.Interactions;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Services;

public class NotifyAboutFailureService(
    IStateService stateService,
    IBotPermissionsService botPermissionsService,
    GetGenericProjectDataHandler getGenericProjectDataHandler,
    GetGenericObserverDataHandler getGenericObserverDataHandler,
    DiscordSocketClient client,
    ILogger<NotifyAboutFailureService> logger
) : INotifyAboutFailureService
{
    private const string Locale = "en-US";

    /// <inheritdoc />
    public async Task NotifyProjectOwner(TaskProgressEvent @event, SocketTextChannel? channel)
    {
        var data = await GetProjectDataAsync(@event.ProjectId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.taskProgress:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithProgressChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyProjectOwner(BulkTaskProgressEvent @event, SocketTextChannel? channel)
    {
        var data = await GetProjectDataAsync(@event.ProjectId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.bulkTaskProgress:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithProgressChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyProjectOwner(EpisodeReleasedEvent @event, SocketTextChannel? channel)
    {
        var data = await GetProjectDataAsync(@event.ProjectId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.episodeRelease:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithReleaseChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyProjectOwner(VolumeReleasedEvent @event, SocketTextChannel? channel)
    {
        var data = await GetProjectDataAsync(@event.ProjectId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.volumeRelease:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithReleaseChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyProjectOwner(BatchReleasedEvent @event, SocketTextChannel? channel)
    {
        var data = await GetProjectDataAsync(@event.ProjectId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.batchRelease:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithReleaseChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyProjectOwner(CongaNotificationEvent @event, SocketTextChannel? channel)
    {
        var data = await GetProjectDataAsync(@event.ProjectId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.congaNotification:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithProgressChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyObserverOwner(
        TaskProgressObserverEvent @event,
        SocketTextChannel? channel
    )
    {
        var data = await GetObserverDataAsync(@event.ObserverId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.observer.taskProgress:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithProgressChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.observer.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyObserverOwner(
        BulkTaskProgressObserverEvent @event,
        SocketTextChannel? channel
    )
    {
        var data = await GetObserverDataAsync(@event.ObserverId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.observer.bulkTaskProgress:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithProgressChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.observer.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyObserverOwner(
        EpisodeReleasedObserverEvent @event,
        SocketTextChannel? channel
    )
    {
        var data = await GetObserverDataAsync(@event.ObserverId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.observer.episodeRelease:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithReleaseChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.observer.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyObserverOwner(
        VolumeReleasedObserverEvent @event,
        SocketTextChannel? channel
    )
    {
        var data = await GetObserverDataAsync(@event.ObserverId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.observer.volumeRelease:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithReleaseChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.observer.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    /// <inheritdoc />
    public async Task NotifyObserverOwner(
        BatchReleasedObserverEvent @event,
        SocketTextChannel? channel
    )
    {
        var data = await GetObserverDataAsync(@event.ObserverId);
        if (data is null)
            return;
        var (pData, owner) = data.Value;

        var stateId = await stateService.SaveStateAsync(@event);
        var buttonId = $"nino.retry.observer.batchRelease:{stateId}";

        var b = new StringBuilder();
        if (channel is not null)
        {
            b.AppendLine(T("publish.failed.body", Locale, $"<#{channel.Id}>"));
            b.AppendLine();
            b = WithReleaseChannelInfo(b, channel);
        }
        else
        {
            b.AppendLine(T("publish.failed.body", Locale, "[???]"));
            b.AppendLine(T("publish.failed.channelNotFound", Locale));
        }

        var embed = new EmbedBuilder()
            .WithProjectInfo(pData, Locale)
            .WithTitle(T("publish.failed.observer.title", Locale))
            .WithDescription(b.ToString())
            .Build();

        var component = new ComponentBuilder()
            .WithButton(T("button.retry", Locale), buttonId, ButtonStyle.Danger)
            .Build();

        await owner.SendMessageAsync(embed: embed, components: component);
    }

    private async Task<(GetGenericProjectDataResponse, IUser)?> GetProjectDataAsync(
        ProjectId projectId
    )
    {
        var request = await getGenericProjectDataHandler.HandleAsync(
            new GetGenericProjectDataQuery(projectId)
        );
        if (!request.IsSuccess)
        {
            logger.LogWarning("Failed to fetch data for project {ProjectId}", projectId);
            return null;
        }
        var pData = request.Value;
        if (pData.Owner.DiscordId is null)
        {
            logger.LogWarning("No Discord ID for user {UserId}", pData.Owner.Id);
            return null;
        }
        var owner = await client.GetUserAsync(pData.Owner.DiscordId.Value);
        if (owner is null)
        {
            logger.LogWarning("Discord user for {UserId} not found", pData.Owner.Id);
            return null;
        }
        return (pData, owner);
    }

    private async Task<(GetGenericProjectDataResponse, IUser)?> GetObserverDataAsync(
        ObserverId observerId
    )
    {
        var request = await getGenericObserverDataHandler.HandleAsync(
            new GetGenericObserverDataQuery(observerId)
        );
        if (!request.IsSuccess)
        {
            logger.LogWarning("Failed to fetch data for observer {ObserverId}", observerId);
            return null;
        }
        var oData = request.Value;
        if (oData.Owner.DiscordId is null)
        {
            logger.LogWarning("No Discord ID for user {UserId}", oData.Owner.Id);
            return null;
        }
        var owner = await client.GetUserAsync(oData.Owner.DiscordId.Value);
        if (owner is null)
        {
            logger.LogWarning("Discord user for {UserId} not found", oData.Owner.Id);
            return null;
        }
        return (oData.ProjectData, owner);
    }

    private StringBuilder WithProgressChannelInfo(StringBuilder body, SocketTextChannel channel)
    {
        var passFail = new Dictionary<bool, string> { [true] = "✅ ", [false] = "❌ " };

        var p = botPermissionsService.GetChannelPermissions(channel.Id)!.Value;
        body.AppendLine(passFail[p.ViewChannel] + T("nino.debug.channel.view", Locale));
        body.AppendLine(passFail[p.SendMessages] + T("nino.debug.channel.send", Locale));
        body.AppendLine(passFail[p.EmbedLinks] + T("nino.debug.channel.embed", Locale));
        return body;
    }

    private StringBuilder WithReleaseChannelInfo(StringBuilder body, SocketTextChannel channel)
    {
        var passFail = new Dictionary<bool, string> { [true] = "✅ ", [false] = "❌ " };
        var passWarn = new Dictionary<bool, string> { [true] = "✅ ", [false] = "⚠️ " };

        var p = botPermissionsService.GetChannelPermissions(channel.Id)!.Value;
        body.AppendLine(passFail[p.ViewChannel] + T("nino.debug.channel.view", Locale));
        body.AppendLine(passFail[p.SendMessages] + T("nino.debug.channel.send", Locale));
        body.AppendLine(passFail[p.EmbedLinks] + T("nino.debug.channel.embed", Locale));
        body.AppendLine(passFail[p.MentionEveryone] + T("nino.debug.channel.mention", Locale));
        body.AppendLine(
            passWarn[channel.ChannelType is ChannelType.News]
                + T("nino.debug.channel.crosspost", Locale)
        );
        return body;
    }
}
