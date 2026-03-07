// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class PinchHitterConfiguration : IEntityTypeConfiguration<PinchHitter>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PinchHitter> pinchHitter)
    {
        pinchHitter.HasKey(p => p.Id);

        pinchHitter
            .HasOne(p => p.Episode)
            .WithMany(e => e.PinchHitters)
            .HasForeignKey(p => p.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        pinchHitter
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        pinchHitter.Navigation(p => p.User).AutoInclude();
    }
}
