// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nino.Discord;

public sealed class DiscordBotHostedService(
    IOptions<DiscordOptions> options,
    ILogger<DiscordBotHostedService> logger
) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken) { }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) { }
}
