// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Services;

public interface IUserIdentityService
{
    Task<Guid> GetOrCreateUserByDiscordIdAsync(ulong discordId);
}
