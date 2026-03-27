// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.EntityConfig;

public sealed class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Task> task)
    {
        task.HasKey(t => t.Id);

        task.HasOne(t => t.Episode)
            .WithMany(e => e.Tasks)
            .HasForeignKey(t => t.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        task.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        task.Navigation(t => t.Assignee).AutoInclude();
    }
}
