
import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { getAdditionalStaff, getEpisode, getTask } from "../actions/getters";

export const RemoveAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episodeNumber = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;

  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);
  let project = projects[projectName];
  
  const { id: episodeId, episode } = getEpisode(project, episodeNumber);
  if (!episode) return fail(t('error.noSuchEpisode', { lng, episodeNumber }), interaction);
  
  let { id: addStaffId } = getAdditionalStaff(episode, abbreviation);
  if (addStaffId) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId}/additionalStaff`).child(addStaffId).remove();
    
    let { id: taskId } = getTask(episode, abbreviation);
    if (taskId) // idk why this would be undefined but here's a check!
      db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId}/tasks`).child(taskId).remove();
    
  } else {
    return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('additionalStaff.removed', { lng, abbreviation, episode: episodeNumber }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}