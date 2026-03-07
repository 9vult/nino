// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Services;

/// <summary>
/// Provides methods for resolving and creating internal domain IDs
/// from external entity IDs, and for performing the reverse lookup.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Get the <see cref="User"/> ID for a given Discord ID.
    /// </summary>
    /// <param name="discordId">Discord user ID.</param>
    /// <param name="discordUsername">Discord username.</param>
    /// <returns>The corresponding <see cref="UserId"/>.</returns>
    /// <remarks>Creates a new <see cref="User"/> if not found.</remarks>
    Task<UserId> GetOrCreateUserByDiscordIdAsync(ulong discordId, string discordUsername);

    /// <summary>
    /// Get the <see cref="Group"/> ID for a given Discord ID.
    /// </summary>
    /// <param name="discordId">Discord group ID.</param>
    /// <returns>The corresponding <see cref="GroupId"/>.</returns>
    /// <remarks>Creates a new <see cref="Group"/> if not found.</remarks>
    Task<GroupId> GetOrCreateGroupByDiscordIdAsync(ulong discordId);

    /// <summary>
    /// Get the <see cref="Channel"/> ID for a given Discord ID.
    /// </summary>
    /// <param name="discordId">Discord channel ID.</param>
    /// <returns>The corresponding <see cref="ChannelId"/>.</returns>
    /// <remarks>Creates a new <see cref="Channel"/> if not found.</remarks>
    Task<ChannelId> GetOrCreateChannelByDiscordIdAsync(ulong discordId);

    /// <summary>
    /// Get the <see cref="MentionRole"/> ID for a given Discord ID.
    /// </summary>
    /// <param name="discordId">Discord mention role ID.</param>
    /// <returns>The corresponding <see cref="MentionRoleId"/>.</returns>
    /// <remarks>Creates a new <see cref="MentionRole"/> if not found.</remarks>
    Task<MentionRoleId> GetOrCreateMentionRoleByDiscordIdAsync(ulong discordId);

    /// <summary>
    /// Get the Discord user ID for a given <see cref="User"/> ID.
    /// </summary>
    /// <param name="userId">The internal <see cref="UserId"/>.</param>
    /// <returns>The Discord user ID, or <see langword="null"/> if not found.</returns>
    Task<ulong?> GetDiscordUserIdAsync(UserId userId);

    /// <summary>
    /// Get the Discord group ID for a given <see cref="Group"/> ID.
    /// </summary>
    /// <param name="groupId">The internal <see cref="GroupId"/>.</param>
    /// <returns>The Discord group ID, or <see langword="null"/> if not found.</returns>
    Task<ulong?> GetDiscordGroupIdAsync(GroupId groupId);

    /// <summary>
    /// Get the Discord channel ID for a given <see cref="Channel"/> ID.
    /// </summary>
    /// <param name="channelId">The internal <see cref="ChannelId"/>.</param>
    /// <returns>The Discord channel ID, or <see langword="null"/> if not found.</returns>
    Task<ulong?> GetDiscordChannelIdAsync(ChannelId channelId);

    /// <summary>
    /// Get the Discord mention role ID for a given <see cref="MentionRole"/> ID.
    /// </summary>
    /// <param name="mentionRoleId">The internal <see cref="MentionRoleId"/>.</param>
    /// <returns>The Discord mention role ID, or <see langword="null"/> if not found.</returns>
    Task<ulong?> GetDiscordMentionRoleIdAsync(MentionRoleId mentionRoleId);
}
