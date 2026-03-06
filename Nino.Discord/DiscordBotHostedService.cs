// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Hosting;

namespace Nino.Discord;

public sealed class DiscordBotHostedService : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
