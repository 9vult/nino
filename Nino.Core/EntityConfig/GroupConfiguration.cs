// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Group> group)
    {
        group.HasKey(g => g.Id);

        group
            .HasOne(g => g.Configuration)
            .WithOne(c => c.Group)
            .HasForeignKey<Configuration>(c => c.GroupId);
    }
}
