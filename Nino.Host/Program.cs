// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nino.Core;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Events;
using Nino.Discord.Services;
using Nino.Host;
using NLog.Extensions.Hosting;

LogProvider.Setup();
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (context, config) =>
        {
            config
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true
                );
        }
    )
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(LogLevel.Trace);
    })
    .UseNLog()
    .ConfigureServices(
        (context, services) =>
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection"))
            );
            services.AddDiscordBotService(context.Configuration);

            // Event bus
            services.AddSingleton<IEventBus, InMemoryEventBus>();

            // Action handlers
            services.AddScoped<ProjectDeleteHandler>();
        }
    );

var host = builder.Build();
await host.RunAsync();
