import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const AddEpisodeCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const number = options.getNumber('episode')!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  ref.update({ length: projects[project].length + 1 });
  
  let epref = ref.child('episodes').push({
    number: number,
    done: false,
    additionalStaff: [],
    tasks: [],
    updated: 0,
    airReminderPosted: false
  });
  
  for (let pos in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[pos]
    epref.child('tasks').push({
      abbreviation: staffObj.role.abbreviation,
      done: false
    });
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('episode.added', { lng, number, project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}