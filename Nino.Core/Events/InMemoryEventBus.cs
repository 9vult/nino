// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.DependencyInjection;

namespace Nino.Core.Events;

public sealed class InMemoryEventBus(IServiceScopeFactory scopeFactory) : IEventBus
{
    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        using var scope = scopeFactory.CreateScope();

        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event);
        }
    }
}
