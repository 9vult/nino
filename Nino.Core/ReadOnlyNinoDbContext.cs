// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;

namespace Nino.Core;

public sealed class ReadOnlyNinoDbContext : NinoDbContext
{
    public ReadOnlyNinoDbContext(DbContextOptions<NinoDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }
}
