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

        project.Property(p => p.Nickname);
        project.OwnsMany(
            p => p.Aliases,
            b =>
            {
                b.ToTable("Aliases");
                b.Property(a => a.Value);
            }
        );

        project.OwnsMany(
            p => p.Administrators,
            b =>
            {
                b.WithOwner().HasForeignKey("ProjectId");
                b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                b.Navigation(a => a.User).AutoInclude();
            }
        );

        project.Navigation(p => p.TemplateStaff).AutoInclude();
        project.Navigation(p => p.Administrators).AutoInclude();

        project
            .HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        project.Navigation(p => p.Owner).AutoInclude();

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
        project.Navigation(p => p.ProjectChannel).AutoInclude();

        project
            .HasOne(p => p.UpdateChannel)
            .WithMany()
            .HasForeignKey(p => p.UpdateChannelId)
            .OnDelete(DeleteBehavior.Restrict);
        project.Navigation(p => p.UpdateChannel).AutoInclude();

        project
            .HasOne(p => p.ReleaseChannel)
            .WithMany()
            .HasForeignKey(p => p.ReleaseChannelId)
            .OnDelete(DeleteBehavior.Restrict);
        project.Navigation(p => p.ReleaseChannel).AutoInclude();

        project
            .HasOne(p => p.AirNotificationRole)
            .WithMany()
            .HasForeignKey(p => p.AirNotificationRoleId)
            .OnDelete(DeleteBehavior.Restrict);
        project.Navigation(p => p.AirNotificationRole).AutoInclude();

        project
            .HasOne(p => p.AirNotificationUser)
            .WithMany()
            .HasForeignKey(p => p.AirNotificationUserId)
            .OnDelete(DeleteBehavior.Restrict);
        project.Navigation(p => p.AirNotificationUser).AutoInclude();

        project
            .HasOne(p => p.DelegateObserver)
            .WithMany()
            .HasForeignKey(p => p.DelegateObserverId)
            .OnDelete(DeleteBehavior.Restrict);
        project.Navigation(p => p.DelegateObserver).AutoInclude();
    }
}
