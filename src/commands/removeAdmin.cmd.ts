import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { fail } from "../actions/fail.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const RemoveAdminCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { member, options, guildId, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const subcommand = options.getSubcommand()!;

  switch (subcommand) {
    case 'guild_admin':
    {
      if (!(member as GuildMember)?.permissions.has(PermissionsBitField.Flags.Administrator)) {
        return fail(t('notAdmin', { lng }), interaction);
      }
      let staff = (options.getMember('member')! as GuildMember).id;
      
      let ref = db.ref(`/Configuration/${guildId}`);
      if (dbdata.configuration[guildId].administrators)
        ref.update({ administrators: dbdata.configuration[guildId].administrators.filter(a => a !== staff) });

      let staffMention = `<@${staff}>`;
      let embed = new EmbedBuilder()
        .setTitle(t('projectModificationTitle', { lng }))
        .setDescription(t('removeAdminGuild', { lng, staff: staffMention }))
        .setColor(0xd797ff);
      await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
      break;
    }

    case 'project_admin':
    {
      let alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
      let staff = (options.getMember('member')! as GuildMember).id;

      let verification = await VerifyInteraction(dbdata, interaction, alias, true, true); // exclude admins
      if (!verification) return;
      const { projects, project } = InteractionData(dbdata, interaction, alias);
    
      let ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);
      ref.update({ administrators: projects[project].administrators.filter(a => a !== staff) });
    
      let staffMention = `<@${staff}>`;
      let embed = new EmbedBuilder()
        .setTitle(t('projectModificationTitle', { lng }))
        .setDescription(t('removeAdmin', { lng, staff: staffMention, project }))
        .setColor(0xd797ff);
      await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
    break;
    }
  }
}