
import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";

export const RemoveAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale } = interaction;

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
    return fail(interp(GetStr(dbdata.i18n, 'noSuchTask', interaction.locale), { '$ABBREVIATION': abbreviation }), interaction);

  const embed = new EmbedBuilder()
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'additionalStaffRemoved', interaction.locale), { '$ABBREVIATION': abbreviation, '$EPISODE': episode }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}