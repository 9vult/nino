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
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await db.Users.AddAsync(user);

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
            CreatedAt = DateTimeOffset.UtcNow,
            Configuration = Configuration.CreateDefault(),
        };
        await db.Groups.AddAsync(group);

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
            logger.LogTrace("Resolved Discord ID {DiscordId} to group {Group}", discordId, group);
            return existing.Id;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> GetOrCreateChannelByDiscordIdAsync(ulong discordId)
    {
        var channel = await db.Channels.SingleOrDefaultAsync(c => c.DiscordId == discordId);

        if (channel is not null)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to channel {Channel}",
                discordId,
                channel
            );
            return channel.Id;
        }

        channel = new Channel
        {
            Id = Guid.NewGuid(),
            DiscordId = discordId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await db.Channels.AddAsync(channel);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace(
                "Created channel {Channel} for Discord ID {DiscordId}",
                channel,
                discordId
            );
            return channel.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            var existing = await db.Channels.SingleAsync(c => c.DiscordId == discordId);
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to channel {Channel}",
                discordId,
                channel
            );
            return existing.Id;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> GetOrCreateMentionRoleByDiscordIdAsync(ulong discordId)
    {
        var role = await db.MentionRoles.SingleOrDefaultAsync(r => r.DiscordId == discordId);

        if (role is not null)
        {
            logger.LogTrace("Resolved Discord ID {DiscordId} to role {Role}", discordId, role);
            return role.Id;
        }

        role = new MentionRole
        {
            Id = Guid.NewGuid(),
            DiscordId = discordId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        await db.MentionRoles.AddAsync(role);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created role {Role} for Discord ID {DiscordId}", role, discordId);
            return role.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            var existing = await db.MentionRoles.SingleAsync(c => c.DiscordId == discordId);
            logger.LogTrace("Resolved Discord ID {DiscordId} to role {Channel}", discordId, role);
            return existing.Id;
        }
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordUserIdAsync(Guid userId)
    {
        return await db
            .Users.Where(u => u.Id == userId)
            .Select(u => u.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordGroupIdAsync(Guid groupId)
    {
        return await db
            .Groups.Where(u => u.Id == groupId)
            .Select(g => g.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordChannelIdAsync(Guid channelId)
    {
        return await db
            .Channels.Where(u => u.Id == channelId)
            .Select(c => c.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordMentionRoleIdAsync(Guid roleId)
    {
        return await db
            .MentionRoles.Where(u => u.Id == roleId)
            .Select(r => r.DiscordId)
            .SingleOrDefaultAsync();
    }
}
