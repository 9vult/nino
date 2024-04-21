import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";

export const AddAliasCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const newAlias = options.getString('alias')!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, newAlias);

  let aliasProj = await GetAlias(db, dbdata, interaction, newAlias);
  if (aliasProj)
  return fail(interp(GetStr(dbdata.i18n, 'aliasInUse', locale), { '$ALIASPROJ': aliasProj }), interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  if (projects[project].aliases)
    ref.update({ aliases: [...projects[project].aliases, newAlias] });
  else 
    ref.update({ aliases: [newAlias] });

  const embed = new EmbedBuilder()
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'aliasAdded', locale), { '$ALIAS': newAlias, '$PROJECT': project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}