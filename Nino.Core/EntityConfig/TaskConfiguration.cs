// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class TaskConfiguration : IEntityTypeConfiguration<Domain.Entities.Task>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Domain.Entities.Task> task)
    {
        task.HasKey(t => t.Id);

        task.HasOne(t => t.Episode)
            .WithMany(e => e.Tasks)
            .HasForeignKey(t => t.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
