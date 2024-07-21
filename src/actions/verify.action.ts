import { ChatInputCommandInteraction } from "discord.js";
import { DatabaseData } from "../misc/types";
import { fail } from "./fail.action";
import { t } from "i18next";

export const VerifyInteraction = async (dbdata: DatabaseData, interaction: ChatInputCommandInteraction, project: string | undefined, checkOwner: boolean = true, excludeAdmins: boolean = false) => {
  const { user, guildId, locale: lng } = interaction;
  if (guildId == null || !(guildId in dbdata.guilds))
    return await fail(t('noSuchGuild', { lng, guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return await fail(t('noSuchProject', { lng }), interaction);
  
  if (!checkOwner) return true;
  
  const isAdmin = (projects[project].administrators && projects[project].administrators.includes(user!.id))
    || (dbdata.configuration[guildId] && dbdata.configuration[guildId].administrators && dbdata.configuration[guildId].administrators.includes(user!.id));
  const isOwner = projects[project].owner == user!.id;
  
  // Only owner allowed
  if (excludeAdmins && !isOwner)
    return await fail(t('permissionDenied', { lng }), interaction);
  // Owner and admins allowed
  if (!isAdmin && !isOwner)
    return await fail(t('permissionDenied', { lng }), interaction);

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