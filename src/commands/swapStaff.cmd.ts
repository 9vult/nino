import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";

export const SwapStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();
  const locale = interaction.locale;

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  const staff = (options.getMember('member')! as GuildMember).id;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchGuild', locale), { '$GUILDID': guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchproject', interaction.locale), { '$PROJECT': project }), interaction);
  if (projects[project].owner !== user!.id)
    return fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

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
    return fail(interp(GetStr(dbdata.i18n, 'noSuchTask', interaction.locale), { '$ABBREVIATION': abbreviation }), interaction);

  const embed = new EmbedBuilder()
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'swapStaff', locale), { '$STAFF': staff, '$ABBREVIATION': abbreviation }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}