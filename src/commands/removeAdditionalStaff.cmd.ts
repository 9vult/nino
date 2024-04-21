
import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";

export const RemoveAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { commandName, options, user, member, guildId } = interaction;

  await interaction.deferReply();
  const locale = interaction.locale;

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const episode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchGuild', locale), { '$GUILDID': guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchproject', interaction.locale), { '$PROJECT': project }), interaction);
  if (projects[project].owner !== user!.id)
    return fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

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