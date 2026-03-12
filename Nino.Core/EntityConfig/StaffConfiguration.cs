// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Staff> staff)
    {
        staff.HasKey(s => s.Id);

        staff
            .HasOne(s => s.Project)
            .WithMany(p => p.KeyStaff)
            .HasForeignKey(s => s.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        staff
            .HasOne(s => s.Episode)
            .WithMany(e => e.AdditionalStaff)
            .HasForeignKey(s => s.EpisodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        staff
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        staff.Navigation(s => s.User).AutoInclude();

        staff.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Staff_SingleOwner",
                "(ProjectId IS NULL) != (EpisodeId IS NULL)"
            )
        );
    }
}
