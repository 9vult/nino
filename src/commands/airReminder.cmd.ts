import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const AirReminderCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, member, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const subcommand = options.getSubcommand()!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { project } = InteractionData(dbdata, interaction, alias);

  switch (subcommand) {
    case 'enable':
      const channelId = options.getChannel('updatechannel')!.id;
      const role = options.getRole('role');
      const roleId = role ? (role.name == "@everyone" ? "@everyone" : role.id) : '';
      db.ref(`/Projects/${guildId}/${project}`).update({ airReminderEnabled: true, airReminderChannel: channelId, airReminderRole: roleId });
      break;
    case 'disable':
      db.ref(`/Projects/${guildId}/${project}`).update({ airReminderEnabled: false, airReminderChannel: '', airReminderRole: '' });
      break;
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('guildConfiguration.saved', { lng }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}