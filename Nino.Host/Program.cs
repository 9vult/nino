// SPDX-License-Identifier: MPL-2.0

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nino.Core;
using Nino.Discord;
using Nino.Web;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Nino host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog(
        (services, config) =>
            config
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
    );

    builder.Services.AddCore(builder.Configuration);
    builder.Services.AddDiscordBot(builder.Configuration);
    builder.Services.AddWebApi(builder.Configuration);

    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NinoDbContext>();
        await db.Database.MigrateAsync();
    }

    using var globalLogScope = host
        .Services.GetRequiredService<ILogger<Program>>()
        .BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = "Global",
                ["InteractionType"] = "System",
            }
        );

    // Configure API
    host.UseWebApi();

    // Start everything
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Nino host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
