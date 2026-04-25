// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;

namespace Nino.Discord;

public sealed class DiscordOptions
{
    public const string Section = "Discord";

    /// <summary>
    /// Discord application token.
    /// </summary>
    [Required]
    public required string Token { get; set; }

    /// <summary>
    /// User ID of the bot owner
    /// </summary>
    [Required]
    public required ulong OwnerId { get; set; }

    /// <summary>
    /// Development guild ID. Set to <see langword="null"/> in production.
    /// </summary>
    public ulong? GuildId { get; set; }

    /// <summary>
    /// If the bot is currently undergoing maintenance
    /// </summary>
    public bool MaintenanceGate { get; set; }
}
