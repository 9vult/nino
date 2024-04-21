import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";

export const AddAliasCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();
  const locale = interaction.locale;

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const alias = options.getString('alias')!;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchGuild', locale), { '$GUILDID': guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchproject', interaction.locale), { '$PROJECT': project }), interaction);
  if (projects[project].owner !== user!.id)
    return fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

  let aliasProj = await GetAlias(db, dbdata, interaction, alias);
  if (aliasProj)
  return fail(interp(GetStr(dbdata.i18n, 'aliasInUse', locale), { '$ALIASPROJ': aliasProj }), interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  if (projects[project].aliases)
    ref.update({ aliases: [...projects[project].aliases, alias] });
  else 
    ref.update({ aliases: [alias] });

  const embed = new EmbedBuilder()
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'aliasAdded', locale), { '$ALIAS': alias, '$PROJECT': project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}