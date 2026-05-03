// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Events;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Services;

public sealed class IdentityService(
    NinoDbContext db,
    IEventBus eventBus,
    ILogger<IdentityService> logger
) : IIdentityService
{
    /// <inheritdoc />
    public async Task<UserId> GetOrCreateUserByDiscordIdAsync(
        ulong discordId,
        string discordUsername
    )
    {
        var resolvedId = await db
            .Users.AsNoTracking()
            .Where(u => u.DiscordId == discordId)
            .Select(u => (UserId?)u.Id)
            .SingleOrDefaultAsync();

        if (resolvedId is not null)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to user {UserId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }

        var user = new User { DiscordId = discordId, Name = discordUsername };
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
            resolvedId = await db
                .Users.AsNoTracking()
                .Where(u => u.DiscordId == discordId)
                .Select(u => u.Id)
                .SingleAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to user {UserId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }
    }

    /// <inheritdoc />
    public async Task<UserId> GetOrCreateUserByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .Users.AsNoTracking()
            .Where(u => u.DiscordId == discordId)
            .Select(u => (UserId?)u.Id)
            .SingleOrDefaultAsync();

        if (resolvedId is not null)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to user {UserId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }

        var user = new User { DiscordId = discordId, Name = string.Empty };
        await db.Users.AddAsync(user);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created user {User} for Discord ID {DiscordId}", user, discordId);

            // Fire event for name enrichment
            await eventBus.PublishAsync(new PartialUserCreatedFromDiscordEvent(user.Id, discordId));
            return user.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            resolvedId = await db
                .Users.AsNoTracking()
                .Where(u => u.DiscordId == discordId)
                .Select(u => u.Id)
                .SingleAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to user {UserId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }
    }

    /// <inheritdoc />
    public async Task<GroupId> GetOrCreateGroupByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .Groups.AsNoTracking()
            .Where(g => g.DiscordId == discordId)
            .Select(g => (GroupId?)g.Id)
            .SingleOrDefaultAsync();

        if (resolvedId is not null)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to group {GroupId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }

        var config = new Configuration();

        var group = new Group
        {
            DiscordId = discordId,
            Configuration = config,
            ConfigurationId = config.Id,
        };
        config.GroupId = group.Id;
        await db.Groups.AddAsync(group);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created group {Group} for Discord ID {DiscordId}", group, discordId);

            // Fire event for name enrichment
            await eventBus.PublishAsync(
                new PartialGroupCreatedFromDiscordEvent(group.Id, discordId)
            );

            return group.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            resolvedId = await db
                .Groups.AsNoTracking()
                .Where(g => g.DiscordId == discordId)
                .Select(g => g.Id)
                .SingleOrDefaultAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to group {GroupId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }
    }

    /// <inheritdoc />
    public async Task<ChannelId> GetOrCreateChannelByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .Channels.AsNoTracking()
            .Where(c => c.DiscordId == discordId)
            .Select(c => (ChannelId?)c.Id)
            .SingleOrDefaultAsync();

        if (resolvedId is not null)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to channel {ChannelId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }

        var channel = new Channel { DiscordId = discordId };
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
            resolvedId = await db
                .Channels.AsNoTracking()
                .Where(c => c.DiscordId == discordId)
                .Select(c => c.Id)
                .SingleOrDefaultAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to channel {ChannelId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }
    }

    /// <inheritdoc />
    public async Task<RoleId> GetOrCreateRoleByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .Roles.AsNoTracking()
            .Where(r => r.DiscordId == discordId)
            .Select(r => (RoleId?)r.Id)
            .SingleOrDefaultAsync();

        if (resolvedId is not null)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to role {RoleId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }

        var role = new Role { DiscordId = discordId };
        await db.Roles.AddAsync(role);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace("Created role {Role} for Discord ID {DiscordId}", role, discordId);
            return role.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            resolvedId = await db
                .Roles.AsNoTracking()
                .Where(r => r.DiscordId == discordId)
                .Select(r => r.Id)
                .SingleOrDefaultAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to role {RoleId}",
                discordId,
                resolvedId.Value
            );
            return resolvedId.Value;
        }
    }

    /// <inheritdoc />
    public async Task<UserId?> GetUserByDiscordIdAsync(ulong discordId)
    {
        return await db
            .Users.AsNoTracking()
            .Where(g => g.DiscordId == discordId)
            .Select(u => (UserId?)u.Id)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<GroupId?> GetGroupByDiscordIdAsync(ulong discordId)
    {
        return await db
            .Groups.AsNoTracking()
            .Where(g => g.DiscordId == discordId)
            .Select(g => (GroupId?)g.Id)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordUserIdAsync(UserId userId)
    {
        return await db
            .Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordGroupIdAsync(GroupId groupId)
    {
        return await db
            .Groups.AsNoTracking()
            .Where(g => g.Id == groupId)
            .Select(g => g.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordChannelIdAsync(ChannelId channelId)
    {
        return await db
            .Channels.AsNoTracking()
            .Where(c => c.Id == channelId)
            .Select(c => c.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordRoleIdAsync(RoleId roleId)
    {
        return await db
            .Roles.AsNoTracking()
            .Where(r => r.Id == roleId)
            .Select(r => r.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUserNameAsync(UserId userId, string newUsername)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return false;

        user.Name = newUsername;
        await db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateGroupNameAsync(GroupId groupId, string newGroupName)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group is null)
            return false;

        group.Name = newGroupName;
        await db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<string?> GetUserNameAsync(UserId userId)
    {
        return await db.Users.Where(u => u.Id == userId).Select(u => u.Name).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public Task<string?> GetGroupNameAsync(GroupId groupId)
    {
        return db.Groups.Where(g => g.Id == groupId).Select(g => g.Name).FirstOrDefaultAsync();
    }
}
