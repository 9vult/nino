// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Services;

public interface IIdentityService
{
    /// <summary>
    /// Get the <see cref="User"/> ID for a given Discord ID
    /// </summary>
    /// <param name="discordId">Discord user ID</param>
    /// <param name="discordName">Name of the user</param>
    /// <returns>User ID</returns>
    /// <remarks>Creates a new <see cref="User"/> if not found</remarks>
    Task<Guid> GetOrCreateUserByDiscordIdAsync(ulong discordId, string discordName);

    /// <summary>
    /// Get the <see cref="Group"/> ID for a given Discord ID
    /// </summary>
    /// <param name="discordId">Discord guild ID</param>
    /// <returns>Group ID</returns>
    /// <remarks>Creates a new <see cref="Group"/> if not found</remarks>
    Task<Guid> GetOrCreateGroupByDiscordIdAsync(ulong discordId);

    /// <summary>
    /// Get the <see cref="Channel"/> ID for a given Discord ID
    /// </summary>
    /// <param name="discordId">Discord channel ID</param>
    /// <returns>Channel ID</returns>
    /// <remarks>Creates a new <see cref="Channel"/> if not found</remarks>
    Task<Guid> GetOrCreateChannelByDiscordIdAsync(ulong discordId);

    /// <summary>
    /// Get the <see cref="MentionRole"/> ID for a given Discord ID
    /// </summary>
    /// <param name="discordId">Discord role ID</param>
    /// <returns>Role ID</returns>
    /// <remarks>Creates a new <see cref="MentionRole"/> if not found</remarks>
    Task<Guid> GetOrCreateMentionRoleByDiscordIdAsync(ulong discordId);

    /// <summary>
    /// Get a <see cref="User"/>'s Discord ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User's Discord ID, or <see langword="null"/> if it does not exist</returns>
    Task<ulong?> GetDiscordUserIdAsync(Guid userId);

    /// <summary>
    /// Get a <see cref="Group"/>'s Discord ID
    /// </summary>
    /// <param name="groupId">Group ID</param>
    /// <returns>Group's Discord ID, or <see langword="null"/> if it does not exist</returns>
    Task<ulong?> GetDiscordGroupIdAsync(Guid groupId);

    /// <summary>
    /// Get a <see cref="Channel"/>'s Discord ID
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <returns>Channel's Discord ID, or <see langword="null"/> if it does not exist</returns>
    Task<ulong?> GetDiscordChannelIdAsync(Guid channelId);

    /// <summary>
    /// Get a <see cref="MentionRole"/>'s Discord ID
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Role's Discord ID, or <see langword="null"/> if it does not exist</returns>
    Task<ulong?> GetDiscordMentionRoleIdAsync(Guid roleId);
}
