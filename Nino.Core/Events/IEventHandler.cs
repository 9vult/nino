// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

public interface IEventHandler<in TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Handle the event
    /// </summary>
    /// <param name="event">Event to handle</param>
    Task HandleAsync(TEvent @event);
}
