// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nino.Core;
using Nino.Discord;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting Nino host");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog(
        (services, config) =>
            config
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
    );

    builder.Services.AddCore(builder.Configuration);
    builder.Services.AddDiscordBot(builder.Configuration);

    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NinoDbContext>();
        await db.Database.MigrateAsync();
    }

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
