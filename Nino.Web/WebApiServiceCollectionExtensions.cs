// SPDX-License-Identifier: MPL-2.0

using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nino.Web;

public static class WebApiServiceCollectionExtensions
{
    public static IServiceCollection AddWebApi(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(WebApiServiceCollectionExtensions).Assembly);
        services.AddOpenApi();

        // Configure API versioning
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });

        // Configure localhost CORS whitelist
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
            });
        });

        return services;
    }
}
