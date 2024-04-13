
import { AutocompleteInteraction, ChatInputCommandInteraction } from "discord.js";
import { DatabaseData, ObserverAliasResult } from "../misc/types";
import { Database } from "@firebase/database-types";

export const GetAlias = async (db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction | AutocompleteInteraction, query: string, obsGuild: string | undefined = undefined) => {
  if (!interaction.isCommand() && !interaction.isAutocomplete()) return;
  const guildId = obsGuild ? obsGuild : interaction.guildId;

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
};

export const GetObserverAlias = async (db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction | AutocompleteInteraction, query: string): Promise<ObserverAliasResult> => {
  if (!interaction.isCommand() && !interaction.isAutocomplete() || !interaction.guildId)
    return { guildId: undefined, project: undefined };
  const home = await GetAlias(db, dbdata, interaction, query);
  
  if (home) return {
    guildId: interaction.guildId,
    project: home
  };

  if (!dbdata.observers || !dbdata.observers[interaction.guildId])
    return { guildId: undefined, project: undefined };

  for (let observingGuild in dbdata.observers[interaction.guildId]) {
    let away = await GetAlias(db, dbdata, interaction, query, observingGuild);
    if (away) return {
      guildId: observingGuild,
      project: away
    }
  }
  return { guildId: undefined, project: undefined }; // Fall-through
};
