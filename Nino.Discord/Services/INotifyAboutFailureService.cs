// SPDX-License-Identifier: MPL-2.0

using Discord.WebSocket;
using Nino.Core.Events;

namespace Nino.Discord.Services;

public interface INotifyAboutFailureService
{
    Task NotifyProjectOwner(TaskProgressEvent @event, SocketTextChannel? channel);
    Task NotifyProjectOwner(BulkTaskProgressEvent @event, SocketTextChannel? channel);
    Task NotifyProjectOwner(EpisodeReleasedEvent @event, SocketTextChannel? channel);
    Task NotifyProjectOwner(VolumeReleasedEvent @event, SocketTextChannel? channel);
    Task NotifyProjectOwner(BatchReleasedEvent @event, SocketTextChannel? channel);
    Task NotifyProjectOwner(CongaNotificationEvent @event, SocketTextChannel? channel);

    Task NotifyObserverOwner(TaskProgressObserverEvent @event, SocketTextChannel? channel);
    Task NotifyObserverOwner(BulkTaskProgressObserverEvent @event, SocketTextChannel? channel);
    Task NotifyObserverOwner(EpisodeReleasedObserverEvent @event, SocketTextChannel? channel);
    Task NotifyObserverOwner(VolumeReleasedObserverEvent @event, SocketTextChannel? channel);
    Task NotifyObserverOwner(BatchReleasedObserverEvent @event, SocketTextChannel? channel);
}
