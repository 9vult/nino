import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const SwapStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const staff = (options.getMember('member')! as GuildMember).id;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  var found;
  for (let keystaff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[keystaff];
    if (staffObj.role.abbreviation === abbreviation) {
      found = keystaff;
      db.ref(`/Projects/${guildId}/${project}`).child("keyStaff").child(found).update({ id: staff });
      break;
    }
  }

  if (found == undefined)
    return fail(t('noSuchTask', { lng, abbreviation }), interaction);

  const staffMention = `<@$staff}>`;
  const embed = new EmbedBuilder()
    .setTitle(t('projectModificationTitle', { lng }))
    .setDescription(t('swapStaff', { lng, staff: staffMention, abbreviation }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}