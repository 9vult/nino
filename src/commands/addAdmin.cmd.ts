import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const AddAdminCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const staff = (options.getMember('member')! as GuildMember).id;

  let verification = await VerifyInteraction(dbdata, interaction, alias, true, true); // exclude admins
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  let ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  if (projects[project].administrators)
    ref.update({ administrators: [...projects[project].administrators, staff] });
  else 
    ref.update({ administrators: [staff] });

  const staffMention = `<@${staff}>`;
  const embed = new EmbedBuilder()
    .setTitle(t('projectModificationTitle', { lng }))
    .setDescription(t('addAdmin', { lng, staff: staffMention, project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}