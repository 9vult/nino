// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Events;

namespace Nino.Discord.Handlers;

public class TaskCompletedEventHandler : IEventHandler<TaskCompletedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TaskCompletedEvent @event)
    {
        throw new NotImplementedException();
    }
}
