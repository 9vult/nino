// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class AniListResponseConfiguration : IEntityTypeConfiguration<AniListResponse>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AniListResponse> channel)
    {
        channel.HasKey(c => c.Id);
    }
}
