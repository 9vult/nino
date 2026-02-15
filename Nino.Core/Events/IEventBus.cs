// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

/// <summary>
/// Bus for publishing events to clients
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish an event
    /// </summary>
    /// <param name="event">Event to publish</param>
    /// <typeparam name="TEvent">Type of event</typeparam>
    Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : IEvent;
}
