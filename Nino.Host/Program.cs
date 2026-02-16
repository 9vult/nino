// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nino.Core;
using Nino.Core.Actions.Project.Create;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Events;
using Nino.Core.Services;
using Nino.Discord;
using Nino.Host;
using NLog.Extensions.Logging;

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
        logging.SetMinimumLevel(LogLevel.Debug);
        logging.AddFilter<NLogLoggerProvider>("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        logging.AddFilter<NLogLoggerProvider>(
            "Microsoft.EntityFrameworkCore.Database",
            LogLevel.Warning
        );
        logging.AddFilter<NLogLoggerProvider>("System.Net.Http.HttpClient", LogLevel.Warning);
        logging.AddFilter<NLogLoggerProvider>(
            "Microsoft.Extensions.Http.DefaultHttpClientFactory",
            LogLevel.Warning
        );
        logging.AddNLog();
    })
    .ConfigureServices(
        (context, services) =>
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection"))
            );
            services.AddHttpClient();
            services.AddDiscordBotService(context.Configuration);

            // Services
            services.AddSingleton<IEventBus, InMemoryEventBus>();
            services.AddScoped<IAniListService, AniListService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserVerificationService, UserVerificationService>();

            // Action handlers
            services.AddScoped<ProjectResolveHandler>();
            services.AddScoped<ProjectCreateHandler>();
            services.AddScoped<ProjectDeleteHandler>();
        }
    );

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
