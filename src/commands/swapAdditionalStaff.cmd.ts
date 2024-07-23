import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const SwapAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const staff = (options.getMember('member')! as GuildMember).id;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  var found;
  for (let ep in projects[project].episodes)
    if (projects[project].episodes[ep].number == episode) {
      for (let pos in projects[project].episodes[ep].additionalStaff)
        if (projects[project].episodes[ep].additionalStaff[pos].role.abbreviation == abbreviation) {
          found = pos;
          db.ref(`/Projects/${guildId}/${project}/episodes/${ep}`).child("additionalStaff").child(pos).update({ id: staff });
          break;
        }
    }

  if (found == undefined)
    return fail(t('error.noSuchTask', { lng, abbreviation }), interaction);

  const staffMention = `<@${staff}>`;
  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('additionalStaff.swapped', { lng, staff: staffMention, abbreviation, episode }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}