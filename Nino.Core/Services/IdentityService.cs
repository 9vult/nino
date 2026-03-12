// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Services;

public sealed class IdentityService(NinoDbContext db, ILogger<IdentityService> logger)
    : IIdentityService
{
    /// <inheritdoc />
    public async Task<UserId> GetOrCreateUserByDiscordIdAsync(
        ulong discordId,
        string discordUsername
    )
    {
        var resolvedId = await db
            .Users.Where(u => u.DiscordId == discordId)
            .Select(u => u.Id)
            .SingleOrDefaultAsync();

        if (resolvedId != default)
        {
            logger.LogTrace("Resolved Discord ID {DiscordId} to {UserId}", discordId, resolvedId);
            return resolvedId;
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
                .Users.Where(u => u.DiscordId == discordId)
                .Select(u => u.Id)
                .SingleAsync();
            logger.LogTrace("Resolved Discord ID {DiscordId} to {UserId}", discordId, resolvedId);
            return resolvedId;
        }
    }

    /// <inheritdoc />
    public async Task<GroupId> GetOrCreateGroupByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .Groups.Where(g => g.DiscordId == discordId)
            .Select(g => g.Id)
            .SingleOrDefaultAsync();

        if (resolvedId != default)
        {
            logger.LogTrace("Resolved Discord ID {DiscordId} to {GroupId}", discordId, resolvedId);
            return resolvedId;
        }

        var config = Configuration.CreateDefault();

        var group = new Group
        {
            DiscordId = discordId,
            Configuration = config,
            ConfigurationId = config.Id,
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
            resolvedId = await db
                .Groups.Where(g => g.DiscordId == discordId)
                .Select(g => g.Id)
                .SingleOrDefaultAsync();
            logger.LogTrace("Resolved Discord ID {DiscordId} to {GroupId}", discordId, resolvedId);
            return resolvedId;
        }
    }

    /// <inheritdoc />
    public async Task<ChannelId> GetOrCreateChannelByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .Channels.Where(c => c.DiscordId == discordId)
            .Select(c => c.Id)
            .SingleOrDefaultAsync();

        if (resolvedId != default)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to {ChannelId}",
                discordId,
                resolvedId
            );
            return resolvedId;
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
                .Channels.Where(c => c.DiscordId == discordId)
                .Select(c => c.Id)
                .SingleOrDefaultAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to {ChannelId}",
                discordId,
                resolvedId
            );
            return resolvedId;
        }
    }

    /// <inheritdoc />
    public async Task<MentionRoleId> GetOrCreateMentionRoleByDiscordIdAsync(ulong discordId)
    {
        var resolvedId = await db
            .MentionRoles.Where(r => r.DiscordId == discordId)
            .Select(r => r.Id)
            .SingleOrDefaultAsync();

        if (resolvedId != default)
        {
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to {MentionRoleId}",
                discordId,
                resolvedId
            );
            return resolvedId;
        }

        var mentionRole = new MentionRole { DiscordId = discordId };
        await db.MentionRoles.AddAsync(mentionRole);

        try
        {
            await db.SaveChangesAsync();
            logger.LogTrace(
                "Created mention role {MentionRole} for Discord ID {DiscordId}",
                mentionRole,
                discordId
            );
            return mentionRole.Id;
        }
        catch (DbUpdateException)
        {
            // Race condition handler
            resolvedId = await db
                .MentionRoles.Where(r => r.DiscordId == discordId)
                .Select(r => r.Id)
                .SingleOrDefaultAsync();
            logger.LogTrace(
                "Resolved Discord ID {DiscordId} to {MentionRoleId}",
                discordId,
                resolvedId
            );
            return resolvedId;
        }
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordUserIdAsync(UserId userId)
    {
        return await db
            .Users.Where(u => u.Id == userId)
            .Select(u => u.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordGroupIdAsync(GroupId groupId)
    {
        return await db
            .Groups.Where(g => g.Id == groupId)
            .Select(g => g.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordChannelIdAsync(ChannelId channelId)
    {
        return await db
            .Channels.Where(c => c.Id == channelId)
            .Select(c => c.DiscordId)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ulong?> GetDiscordMentionRoleIdAsync(MentionRoleId mentionRoleId)
    {
        return await db
            .MentionRoles.Where(r => r.Id == mentionRoleId)
            .Select(r => r.DiscordId)
            .SingleOrDefaultAsync();
    }
}
