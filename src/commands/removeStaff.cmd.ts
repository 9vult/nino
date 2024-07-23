import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const RemoveStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  let success = false;
  for (let pos in projects[project].keyStaff)
    if (projects[project].keyStaff[pos].role.abbreviation == abbreviation) {
      success = true;
      db.ref(`/Projects/${guildId}/${project}`).child("keyStaff").child(pos).remove();
    }

if (!success)
  return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);


  const episodes = projects[project].episodes;
  for (let key in episodes) {
    for (let task in episodes[key].tasks) {
      if (episodes[key].tasks[task].abbreviation == abbreviation)
        db.ref(`/Projects/${guildId}/${project}/episodes/${key}/tasks`).child(task).remove();
    }
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('keyStaff.removed',  { lng, abbreviation }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}
