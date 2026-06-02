// SPDX-License-Identifier: MPL-2.0

namespace Nino.Discord.Entities;

/// <summary>
/// Permissions the bot has for a channel
/// </summary>
public sealed record BotPermissions(
    bool ViewChannel,
    bool SendMessages,
    bool EmbedLinks,
    bool MentionEveryone
);
