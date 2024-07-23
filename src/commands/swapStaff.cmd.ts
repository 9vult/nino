import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { getKeyStaff } from "../actions/getters";

export const SwapStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const staff = (options.getMember('member')! as GuildMember).id;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;

  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);
  let project = projects[projectName];

  let { id: keyStaffId } = getKeyStaff(project, abbreviation);
  if (keyStaffId) {
    db.ref(`/Projects/${guildId}/${projectName}`).child("keyStaff").child(keyStaffId).update({ id: staff });
  } else {
    return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);
  }

  const staffMention = `<@${staff}>`;
  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('keyStaff.swapped', { lng, staff: staffMention, abbreviation }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}