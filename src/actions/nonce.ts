import { SnowflakeUtil } from "discord.js";

// Snowflakes are limited to 4096 per millisecond,
// so messages beyond this many will be lost.
// Truncated UUIDs could be used instead, but this should be enough.
// This is the upcoming default nonce generator of discord.js.
export const nonce = () => ({ nonce: SnowflakeUtil.generate().toString(), enforceNonce: true });
