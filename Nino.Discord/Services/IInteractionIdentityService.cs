// SPDX-License-Identifier: MPL-2.0

using Discord;
using Nino.Discord.Entities;

namespace Nino.Discord.Services;

public interface IInteractionIdentityService
{
    /// <summary>
    /// Get the user and group involved with an interaction
    /// </summary>
    /// <param name="interaction">Interaction</param>
    /// <returns>User and Group IDs</returns>
    Task<InteractionIdentityResult> GetUserAndGroupAsync(IDiscordInteraction interaction);
}
