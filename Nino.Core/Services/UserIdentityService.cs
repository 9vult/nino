// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Services;

public class UserIdentityService(DataContext db, ILogger<UserIdentityService> logger)
    : IUserIdentityService
{
    /// <inheritdoc />
    public async Task<Guid> GetOrCreateUserByDiscordIdAsync(ulong discordId)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.DiscordId == discordId);

        if (user is not null)
        {
            logger.LogTrace("Resolved Discord ID {DiscordId} to user {UserId}", discordId, user.Id);
            return user.Id;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            DiscordId = discordId,
            CreatedAt = DateTimeOffset.Now,
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created user {UserId} for Discord ID {DiscordId}", user.Id, discordId);
            return user.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            var existing = await db.Users.SingleAsync(u => u.DiscordId == discordId);
            logger.LogTrace("Resolved Discord ID {DiscordId} to user {UserId}", discordId, user.Id);
            return existing.Id;
        }
    }
}
