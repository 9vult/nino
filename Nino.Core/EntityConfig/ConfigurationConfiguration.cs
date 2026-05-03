// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nino.Domain.Entities;

namespace Nino.Core.EntityConfig;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Configuration>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Configuration> config)
    {
        config.HasKey(c => c.Id);

        config.OwnsMany(
            c => c.Administrators,
            b =>
            {
                b.WithOwner().HasForeignKey("ConfigurationId");
                b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired();
                b.Navigation(a => a.User).AutoInclude();
            }
        );
        config.Navigation(c => c.Administrators).AutoInclude();
    }
}
