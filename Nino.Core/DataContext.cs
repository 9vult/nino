// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nino.Core.Dtos;
using Nino.Core.Entities;

namespace Nino.Core;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<MentionRole> MentionRoles { get; set; }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<Observer> Observers { get; set; }
    public DbSet<AniListResponse> AniListCache { get; set; }

    private static readonly ValueConverter<ulong, string> UlongStringConverter = new(
        v => v.ToString(CultureInfo.InvariantCulture),
        v => ulong.Parse(v, CultureInfo.InvariantCulture)
    );

    private static readonly ValueConverter<CongaGraph, string> CongaGraphConverter = new(
        g => JsonSerializer.Serialize(g.Serialize(), JsonSerializerOptions),
        json =>
            CongaGraph.Deserialize(
                JsonSerializer.Deserialize<CongaNodeDto[]>(json, JsonSerializerOptions)
                    ?? Array.Empty<CongaNodeDto>()
            )
    );

    private static readonly ValueConverter<AniListRoot?, string> AniListResponseConverter = new(
        r => JsonSerializer.Serialize(r, JsonSerializerOptions),
        json => JsonSerializer.Deserialize<AniListRoot>(json, JsonSerializerOptions) ?? null
    );

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User

        modelBuilder.Entity<User>(user =>
        {
            user.Property(p => p.DiscordId).HasConversion(UlongStringConverter);
            user.HasIndex(u => u.DiscordId).IsUnique();
        });

        // Channel

        modelBuilder.Entity<Channel>(channel =>
        {
            channel.Property(p => p.DiscordId).HasConversion(UlongStringConverter);
        });

        // Group

        modelBuilder.Entity<Group>(group =>
        {
            group.Property(p => p.DiscordId).HasConversion(UlongStringConverter);

            group.OwnsOne<Configuration>(
                g => g.Configuration,
                c =>
                {
                    c.WithOwner().HasForeignKey("GroupId");
                    c.OwnsMany(
                        e => e.Administrators,
                        s =>
                        {
                            s.WithOwner().HasForeignKey("GroupId");
                            s.HasOne(a => a.User)
                                .WithMany()
                                .HasForeignKey(a => a.UserId)
                                .IsRequired();
                            s.Navigation(a => a.User).AutoInclude();
                        }
                    );
                    c.Navigation(p => p.Administrators).AutoInclude();
                }
            );
            group.Navigation(g => g.Configuration).AutoInclude();
        });

        // Project

        modelBuilder.Entity<Project>(project =>
        {
            project.Property(p => p.Nickname).UseCollation("NOCASE");
            project.OwnsMany(p => p.Aliases, b => b.Property(a => a.Value).UseCollation("NOCASE"));

            project
                .Property(p => p.CongaParticipants)
                .HasConversion(CongaGraphConverter)
                .HasColumnType("TEXT");

            project.OwnsMany(
                e => e.KeyStaff,
                s =>
                {
                    s.WithOwner().HasForeignKey("ProjectId");
                    s.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                    s.Navigation(a => a.User).AutoInclude();
                }
            );
            project.Navigation(p => p.KeyStaff).AutoInclude();

            project.OwnsMany(
                e => e.Administrators,
                s =>
                {
                    s.WithOwner().HasForeignKey("ProjectId");
                    s.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                    s.Navigation(a => a.User).AutoInclude();
                }
            );
            project.Navigation(p => p.Administrators).AutoInclude();

            project
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            project
                .HasOne(p => p.Group)
                .WithMany(g => g.Projects)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            project
                .HasOne(p => p.UpdateChannel)
                .WithMany()
                .HasForeignKey(p => p.UpdateChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            project
                .HasOne(p => p.ReleaseChannel)
                .WithMany()
                .HasForeignKey(p => p.ReleaseChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            project
                .HasOne(p => p.CongaReminderChannel)
                .WithMany()
                .HasForeignKey(p => p.CongaReminderChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            project
                .HasOne(p => p.AirReminderChannel)
                .WithMany()
                .HasForeignKey(p => p.AirReminderChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            project
                .HasOne(p => p.AirReminderRole)
                .WithMany()
                .HasForeignKey(p => p.AirReminderRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            project
                .HasOne(p => p.AirReminderChannel)
                .WithMany()
                .HasForeignKey(p => p.AirReminderChannelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Episode
        modelBuilder.Entity<Episode>(episode =>
        {
            episode
                .HasOne(e => e.Project)
                .WithMany(p => p.Episodes)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            episode
                .HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.NoAction);

            episode.OwnsMany(
                e => e.Tasks,
                t =>
                {
                    t.WithOwner().HasForeignKey("EpisodeId");
                }
            );

            episode.OwnsMany(
                e => e.AdditionalStaff,
                s =>
                {
                    s.WithOwner().HasForeignKey("EpisodeId");
                    s.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                    s.Navigation(a => a.User).AutoInclude();
                }
            );

            episode.OwnsMany(
                e => e.PinchHitters,
                s =>
                {
                    s.WithOwner().HasForeignKey("EpisodeId");
                    s.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                    s.Navigation(a => a.User).AutoInclude();
                }
            );
        });

        // Observer

        modelBuilder.Entity<Observer>(observer =>
        {
            observer
                .HasOne(e => e.Project)
                .WithMany(p => p.Observers)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            observer
                .HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            observer
                .HasOne(e => e.OriginGroup)
                .WithMany()
                .HasForeignKey(e => e.OriginGroupId)
                .OnDelete(DeleteBehavior.Cascade);
            observer
                .HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AniList Cache

        modelBuilder.Entity<AniListResponse>(cached =>
        {
            cached
                .Property(c => c.Data)
                .HasConversion(AniListResponseConverter)
                .HasColumnType("TEXT");
        });
    }
}
