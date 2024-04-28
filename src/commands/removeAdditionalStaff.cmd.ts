
import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const RemoveAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  let success = false;
  for (let ep in projects[project].episodes)
    if (projects[project].episodes[ep].number == episode) {
      epvalue = ep;
      for (let pos in projects[project].episodes[ep].additionalStaff) {
        if (projects[project].episodes[ep].additionalStaff[pos].role.abbreviation == abbreviation) {
          success = true;
          db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/additionalStaff`).child(pos).remove();
        }
      }
      if (success) {
        for (let task in projects[project].episodes[ep].tasks) {
          if (projects[project].episodes[ep].tasks[task].abbreviation == abbreviation)
            db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/tasks`).child(task).remove();
        }
      }
    }
  if (!success)
    return fail(t('noSuchTask', { lng, abbreviation }), interaction);

  const embed = new EmbedBuilder()
    .setTitle(t('projectModificationTitle', { lng }))
    .setDescription(t('additionalStaffRemoved', { lng, abbreviation, episode }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}