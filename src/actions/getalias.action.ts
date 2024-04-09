
import { AutocompleteInteraction, ChatInputCommandInteraction } from "discord.js";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";

export const GetAlias = async (db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction | AutocompleteInteraction, query: string) => {
  if (!interaction.isCommand() && !interaction.isAutocomplete()) return;
  const { guildId } = interaction;

  if (guildId == null || !(guildId in dbdata.guilds))
    return undefined;

  let projects = dbdata.guilds[guildId];

  if ((query in projects)) return query; // Direct match

  for (let projectName in projects) {
    let aliases = projects[projectName].aliases;
    if (!aliases) continue;

    if (aliases.includes(query)) return projectName;
  }
  return undefined; // No match
}