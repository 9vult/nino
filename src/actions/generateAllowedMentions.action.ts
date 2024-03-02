import { MessageMentionOptions } from "discord.js";

/**
 * Generate a list of allowed mentions
 * @param mentions [[Users], [Roles]]
 * @return MessageMentionOptions object
 */
export const generateAllowedMentions = (mentions: Array<Array<string>>): MessageMentionOptions => {
  return {
    parse: [],
    users: mentions[0],
    roles: mentions[1]
  };
}