import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const ConfigurationCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, member, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  if (!(member as GuildMember)?.permissions.has(PermissionsBitField.Flags.Administrator)) {
    return fail(t('notAdmin', { lng }), interaction);
  }

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const subcommand = options.getSubcommand()!;

  const ref = db.ref(`/Configuration/`).child(`${guildId}`);

  switch (subcommand) {
    case 'progress_display':
      const newValue = options.getString('embed_type')!;
      ref.update({ progressDisplay: newValue });
      break;
  }

  const embed = new EmbedBuilder()
    .setTitle(t('configurationTitle', { lng }))
    .setDescription(t('configurationEdited', { lng }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}