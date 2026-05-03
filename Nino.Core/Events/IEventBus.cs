// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Events;

public interface IEventBus
{
    /// <summary>
    /// Publish an event to subscribers
    /// </summary>
    /// <param name="eventIn">Event to publish</param>
    /// <typeparam name="TEvent">Type of the <paramref name="eventIn"/></typeparam>
    Task PublishAsync<TEvent>(TEvent eventIn)
        where TEvent : IEvent;
}
