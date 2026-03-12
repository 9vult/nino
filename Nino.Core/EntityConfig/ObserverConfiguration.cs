// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class ObserverConfiguration : IEntityTypeConfiguration<Observer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Observer> observer)
    {
        observer.HasKey(e => e.Id);

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
    }
}
