import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { getKeyStaff } from "../actions/getters";

export const AddStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const staff = (options.getMember('member')! as GuildMember).id;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const title = options.getString('name')!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);

  let project = projects[projectName];

  let { keyStaff } = getKeyStaff(project, abbreviation);
  if (keyStaff) return fail(t('error.positionExists', { lng }), interaction);

  db.ref(`/Projects/${guildId}/${projectName}`).child("keyStaff").push({
    id: staff,
    role: {
      abbreviation,
      title
    }
  });

  const episodes = project.episodes;
  for (let key in episodes) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${key}`).child("tasks").push({
      abbreviation, done: false
    });
  }

  const staffMention = `<@${staff}>`;
  const embed = new EmbedBuilder()
    .setTitle(t('title.projectCreation', { lng }))
    .setDescription(t('keyStaff.added', { lng, staff: staffMention, abbreviation }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}
