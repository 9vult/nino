// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.DependencyInjection;

namespace Nino.Core.Events;

public sealed class InMemoryEventBus(IServiceScopeFactory scopeFactory) : IEventBus
{
    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent eventIn)
        where TEvent : IEvent
    {
        using var scope = scopeFactory.CreateScope();

        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        await Task.WhenAll(handlers.Select(handler => handler.HandleAsync(eventIn)));
    }
}
