using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nino.Records;

namespace Nino;

public class DataContext : DbContext
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    public DbSet<Project> Projects { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<Observer> Observers { get; set; }
    public DbSet<Configuration> Configurations { get; set; }

    private static readonly ValueConverter<ulong, string> UlongStringConverter = new(
        v => v.ToString(CultureInfo.InvariantCulture),
        v => ulong.Parse(v, CultureInfo.InvariantCulture)
    );

    private static readonly ValueConverter<ulong?, string?> NullableUlongStringConverter = new(
        v => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : null,
        v => v != null ? ulong.Parse(v, CultureInfo.InvariantCulture) : null
    );

    private static readonly ValueConverter<CongaGraph, string> CongaGraphConverter = new(
        g => JsonSerializer.Serialize(g.Serialize(), JsonSerializerOptions),
        json =>
            CongaGraph.Deserialize(
                JsonSerializer.Deserialize<CongaNodeDto[]>(json, JsonSerializerOptions)
                    ?? Array.Empty<CongaNodeDto>()
            )
    );

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=nino-local.db");
        // options.LogTo(Console.WriteLine);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.GuildId).HasConversion(UlongStringConverter);
            entity.Property(p => p.OwnerId).HasConversion(UlongStringConverter);
            entity.Property(p => p.UpdateChannelId).HasConversion(UlongStringConverter);
            entity.Property(p => p.ReleaseChannelId).HasConversion(UlongStringConverter);

            entity
                .Property(p => p.AirReminderChannelId)
                .HasConversion(NullableUlongStringConverter);
            entity.Property(p => p.AirReminderRoleId).HasConversion(NullableUlongStringConverter);
            entity.Property(p => p.AirReminderUserId).HasConversion(NullableUlongStringConverter);
            entity
                .Property(p => p.CongaReminderChannelId)
                .HasConversion(NullableUlongStringConverter);

            entity
                .Property(p => p.CongaParticipants)
                .HasConversion(CongaGraphConverter)
                .HasColumnType("TEXT");

            entity.Property(p => p.Nickname).UseCollation("NOCASE");

            entity.OwnsMany(p => p.Administrators);
            entity.OwnsMany(
                p => p.Aliases,
                b =>
                {
                    b.Property(a => a.Value).UseCollation("NOCASE");
                }
            );
            entity.OwnsMany(
                p => p.KeyStaff,
                b =>
                {
                    b.Property(s => s.UserId).HasConversion(UlongStringConverter);
                }
            );

            entity
                .HasMany(p => p.Episodes)
                .WithOne(e => e.Project)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(p => p.Observers)
                .WithOne(o => o.Project)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(o => o.KeyStaff).AutoInclude();
            entity.Navigation(o => o.Administrators).AutoInclude();
            entity.Navigation(o => o.Aliases).AutoInclude();
        });

        // Episode

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.Property(e => e.GuildId).HasConversion(UlongStringConverter);

            entity.Property(e => e.Updated).HasColumnType("TEXT");

            entity.OwnsMany(e => e.PinchHitters);
            entity.OwnsMany(
                e => e.Tasks,
                b =>
                {
                    b.Property(t => t.Updated).HasColumnType("TEXT");
                    b.Property(t => t.LastReminded).HasColumnType("TEXT");
                }
            );

            entity.OwnsMany(
                e => e.AdditionalStaff,
                b =>
                {
                    b.Property(s => s.UserId).HasConversion(UlongStringConverter);
                }
            );

            entity.Navigation(e => e.AdditionalStaff).AutoInclude();
            entity.Navigation(e => e.Tasks).AutoInclude();
            entity.Navigation(e => e.PinchHitters).AutoInclude();
        });

        // Observer

        modelBuilder.Entity<Observer>(entity =>
        {
            entity.Property(o => o.GuildId).HasConversion(UlongStringConverter);
            entity.Property(o => o.OriginGuildId).HasConversion(UlongStringConverter);
            entity.Property(o => o.OwnerId).HasConversion(UlongStringConverter);
            entity.Property(o => o.RoleId).HasConversion(NullableUlongStringConverter);

            entity.Navigation(o => o.Project).AutoInclude();
        });

        // Configuration

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.Property(o => o.GuildId).HasConversion(UlongStringConverter);
            
            entity.OwnsMany(
                c => c.Administrators,
                b =>
                {
                    b.Property(s => s.UserId).HasConversion(UlongStringConverter);
                }
            );

            entity.Navigation(o => o.Administrators).AutoInclude();
        });
    }
}
