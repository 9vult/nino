// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Project> project)
    {
        project.HasKey(p => p.Id);

        project.Property(p => p.Nickname).UseCollation("NOCASE");
        project.OwnsMany(p => p.Aliases, b => b.Property(a => a.Value).UseCollation("NOCASE"));

        project.OwnsMany(
            p => p.Administrators,
            b =>
            {
                b.WithOwner().HasForeignKey("ProjectId");
                b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                b.Navigation(a => a.User).AutoInclude();
            }
        );

        project.Navigation(p => p.KeyStaff).AutoInclude();
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
            .HasOne(p => p.ProjectChannel)
            .WithMany()
            .HasForeignKey(p => p.ProjectChannelId)
            .OnDelete(DeleteBehavior.Restrict);

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
            .HasOne(p => p.AirNotificationRole)
            .WithMany()
            .HasForeignKey(p => p.AirNotificationRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        project
            .HasOne(p => p.AirNotificationUser)
            .WithMany()
            .HasForeignKey(p => p.AirNotificationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
