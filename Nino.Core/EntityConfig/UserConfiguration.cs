// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> user)
    {
        user.HasKey(u => u.Id);
        user.HasIndex(u => u.DiscordId).IsUnique();
    }
}
