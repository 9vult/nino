// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nino.Core;

/// <summary>
/// Design-time DbContext factory for EF Core to use when creating migrations
/// </summary>
public sealed class NinoDbContextFactory : IDesignTimeDbContextFactory<NinoDbContext>
{
    public NinoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NinoDbContext>()
            .UseSqlite(
                "Data Source=:memory:",
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            )
            .Options;

        return new NinoDbContext(options);
    }
}
