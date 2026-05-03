// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class StateConfiguration : IEntityTypeConfiguration<State>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<State> channel)
    {
        channel.HasKey(c => c.Id);
    }
}
