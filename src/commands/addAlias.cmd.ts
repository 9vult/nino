import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const AddAliasCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const lookupAlias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const alias = options.getString('alias')!;

  let verification = await VerifyInteraction(dbdata, interaction, lookupAlias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, lookupAlias);

  let aliasProj = await GetAlias(db, dbdata, interaction, alias);
  if (aliasProj)
    return fail(t('error.alias.inUse', { lng, aliasproj: aliasProj }), interaction);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
  if (projects[project].aliases)
    ref.update({ aliases: [...projects[project].aliases, alias] });
  else 
    ref.update({ aliases: [alias] });

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('alias.addedAlias', { lng, alias, project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}