// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Events;
using Nino.Core.Events.Episode;

namespace Nino.Discord.Handlers;

public class TaskSkippedEventHandler : IEventHandler<TaskSkippedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TaskSkippedEvent @event)
    {
        throw new NotImplementedException();
    }
}
