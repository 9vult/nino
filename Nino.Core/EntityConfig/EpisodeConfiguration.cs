// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Episode> episode)
    {
        episode.HasKey(e => e.Id);

        episode.Property(e => e.Number).UseCollation("NOCASE");

        episode
            .HasOne(e => e.Project)
            .WithMany(p => p.Episodes)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        episode
            .HasOne(e => e.Group)
            .WithMany()
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        episode.Navigation(e => e.Tasks).AutoInclude();
        episode.Navigation(e => e.PinchHitters).AutoInclude();
        episode.Navigation(e => e.AdditionalStaff).AutoInclude();
    }
}
