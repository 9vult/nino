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
}
