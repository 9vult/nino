// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Services;

public class IdentityService(DataContext db, ILogger<IdentityService> logger) : IIdentityService
{
    /// <inheritdoc />
    public async Task<Guid> GetOrCreateUserByDiscordIdAsync(ulong discordId, string discordName)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.DiscordId == discordId);

        if (user is not null)
        {
            logger.LogTrace("Resolved Discord ID {DiscordId} to user {User}", discordId, user);
            return user.Id;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            Name = discordName,
            DiscordId = discordId,
            CreatedAt = DateTimeOffset.Now,
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created user {User} for Discord ID {DiscordId}", user, discordId);
            return user.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            var existing = await db.Users.SingleAsync(u => u.DiscordId == discordId);
            logger.LogTrace("Resolved Discord ID {DiscordId} to user {User}", discordId, user);
            return existing.Id;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> GetOrCreateGroupByDiscordIdAsync(ulong discordId)
    {
        var group = await db.Groups.SingleOrDefaultAsync(g => g.DiscordId == discordId);

        if (group is not null)
        {
            logger.LogTrace("Resolved Discord ID {DiscordId} to group {Group}", discordId, group);
            return group.Id;
        }

        group = new Group
        {
            Id = Guid.NewGuid(),
            DiscordId = discordId,
            CreatedAt = DateTimeOffset.Now,
        };

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created group {Group} for Discord ID {DiscordId}", group, discordId);
            return group.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            var existing = await db.Groups.SingleAsync(g => g.DiscordId == discordId);
            logger.LogTrace("Resolved Discord ID {DiscordId} to group {User}", discordId, group);
            return existing.Id;
        }
    }
}
