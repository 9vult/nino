import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";

export const AddAliasCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const alias = options.getString('alias')!;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  let aliasProj = await GetAlias(db, dbdata, interaction, alias);
  if (aliasProj)
    return fail(`That alias is already in use in ${aliasProj}`, interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  if (projects[project].aliases)
    ref.update({ aliases: [...projects[project].aliases, alias] });
  else 
    ref.update({ aliases: [alias] });

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`I added \`${alias}\` as an alias for \`${project}\` for you.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}