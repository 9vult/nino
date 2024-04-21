import { ChatInputCommandInteraction } from "discord.js";
import { DatabaseData } from "../misc/types";
import { fail } from "./fail.action";
import { interp } from "./interp.action";
import { GetStr } from "./i18n.action";

export const VerifyInteraction = async (dbdata: DatabaseData, interaction: ChatInputCommandInteraction, project: string | undefined, checkOwner: boolean = true) => {
  const { user, guildId, locale } = interaction;
  if (guildId == null || !(guildId in dbdata.guilds))
    return await fail(interp(GetStr(dbdata.i18n, 'noSuchGuild', locale), { '$GUILDID': guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return await fail(interp(GetStr(dbdata.i18n, 'noSuchproject', interaction.locale), { '$PROJECT': project }), interaction);
  
  if (!checkOwner) return true;
  
  if (projects[project].owner !== user!.id)
    return await fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

  return true;
}

export const InteractionData = (dbdata: DatabaseData, interaction: ChatInputCommandInteraction, project: string | undefined) => {
  const { guildId } = interaction;
  if (guildId == null || project == undefined) {
    return {projects: {}, project: '' }; // should be impossible because of verification
  }
  let projects = dbdata.guilds[guildId];
  return {
    projects,
    project
  }
}