// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class StaffConfiguration : IEntityTypeConfiguration<TemplateStaff>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TemplateStaff> staff)
    {
        staff.HasKey(s => s.Id);

        staff
            .HasOne(s => s.Project)
            .WithMany(p => p.TemplateStaff)
            .HasForeignKey(s => s.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        staff
            .HasOne(s => s.Assignee)
            .WithMany()
            .HasForeignKey(s => s.AssigneeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        staff.Navigation(s => s.Assignee).AutoInclude();
    }
}
