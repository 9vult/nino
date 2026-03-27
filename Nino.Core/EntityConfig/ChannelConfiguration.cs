// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Core.EntityConfig;

public sealed class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Channel> channel)
    {
        channel.HasKey(c => c.Id);

        channel.HasData(
            new Channel
            {
                Id = ChannelId.Unset,
                DiscordId = 0UL,
                CreatedAt = DateTimeOffset.Parse("2000-01-01"),
            }
        );
    }
}
