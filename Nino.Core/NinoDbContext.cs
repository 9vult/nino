// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nino.Domain.Dtos;
using Nino.Domain.Dtos.AniList;
using Nino.Domain.Entities;
using Nino.Domain.Entities.Conga;
using Nino.Domain.ValueObjects;
using Vogen;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core;

public class NinoDbContext(DbContextOptions<NinoDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    public DbSet<User> Users { get; init; } = null!;
    public DbSet<Group> Groups { get; init; } = null!;
    public DbSet<Channel> Channels { get; init; } = null!;
    public DbSet<Role> Roles { get; init; } = null!;

    public DbSet<Project> Projects { get; init; } = null!;
    public DbSet<Episode> Episodes { get; init; } = null!;
    public DbSet<Observer> Observers { get; init; } = null!;

    public DbSet<TemplateStaff> TemplateStaff { get; init; } = null!;
    public DbSet<Task> Tasks { get; init; } = null!;

    public DbSet<State> StateCache { get; init; } = null!;
    public DbSet<AniListResponse> AniListCache { get; init; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(NinoDbContext).Assembly);
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        // Vogen
        var voTypes = typeof(UserId)
            .Assembly.GetTypes()
            .Where(t => t.IsValueType && t.GetCustomAttribute<ValueObjectAttribute>() is not null);

        foreach (var type in voTypes)
        {
            var converterType = type.GetNestedType("EfCoreValueConverter");
            if (converterType is not null)
                builder.Properties(type).HaveConversion(converterType);
        }

        // Other converters
        builder.Properties<ulong>().HaveConversion<UlongStringConverter>();
        builder.Properties<CongaGraph>().HaveConversion<CongaGraphConverter>();
        builder.Properties<AniListRoot>().HaveConversion<AniListRootConverter>();
    }

    private class UlongStringConverter()
        : ValueConverter<ulong, string>(
            v => v.ToString(CultureInfo.InvariantCulture),
            v => ulong.Parse(v, CultureInfo.InvariantCulture)
        );

    private class CongaGraphConverter()
        : ValueConverter<CongaGraph, string>(
            graph => JsonSerializer.Serialize(graph.Serialize(), JsonSerializerOptions),
            json =>
                CongaGraph.Deserialize(
                    JsonSerializer.Deserialize<CongaNodeDto[]>(json, JsonSerializerOptions)
                        ?? Array.Empty<CongaNodeDto>()
                )
        );

    private class AniListRootConverter()
        : ValueConverter<AniListRoot?, string>(
            response => JsonSerializer.Serialize(response, JsonSerializerOptions),
            json => JsonSerializer.Deserialize<AniListRoot>(json, JsonSerializerOptions) ?? null
        );
}
