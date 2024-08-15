import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { CheckChannelExists } from "../actions/checkChannelExists.action";
import { CheckChannelPerms } from "../actions/checkChannelPerms.action";
import { info } from "../actions/info.action";

export const EditProjectCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const option = options.getString('option')!;
  let newValue = options.getString('newvalue')!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { project } = InteractionData(dbdata, interaction, alias);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);

  let helperText = '';

  switch (option) {
    case 'Title':
      ref.update({ title: newValue });
      break;
    case 'Poster':
      ref.update({ poster: newValue });
      break;
    case 'MOTD':
      if (newValue == '-')
        ref.update({ motd: '' });
      else
        ref.update({ motd: newValue });
      helperText = t('project.edited.motdHelp', { lng })
      break;
    case 'UpdateChannel':
      newValue = newValue.replace('<#', '').replace('>', '').trim();
      if (!CheckChannelExists(client, newValue)) return fail(t('error.noSuchChannel', { lng }), interaction);
      if (!CheckChannelPerms(client, newValue)) info(t('error.missingChannelPerms', { lng, channel: `<#${newValue}>` }), interaction, client);
      ref.update({ updateChannel: newValue });
      break;
    case 'ReleaseChannel':
      newValue = newValue.replace('<#', '').replace('>', '').trim();
      if (!CheckChannelExists(client, newValue)) return fail(t('error.noSuchChannel', { lng }), interaction);
      if (!CheckChannelPerms(client, newValue, true)) info(t('error.missingChannelPermsRelease', { lng, channel: `<#${newValue}>` }), interaction, client);
      ref.update({ releaseChannel: newValue });
      break;
    case 'AniDB':
      ref.update({ anidb: newValue });
      break;
    case 'AirTime24h':
      const isTime = /^([0-9]{2}):([0-9]{2})$/.test(newValue);
      if (!isTime) return fail(t('error.incorrectAirTimeFormat', { lng }), interaction);
      ref.update({ airTime: newValue });
      break;
    case 'IsPrivate':
      if (!(newValue.toLowerCase() == 'true') && !(newValue.toLowerCase() == 'false')
        && !(newValue.toLowerCase() == 'yes') && !(newValue.toLowerCase() == 'no')) {
          return fail(t('error.incorrectBooleanFormat', { lng }), interaction);
      }
      let truthy = (newValue.toLowerCase() == 'true') || (newValue.toLowerCase() == 'yes');
      ref.update({ isPrivate: truthy });
      break;
  }

  const description = helperText ? `${t('project.edited', { lng, project })}\n${helperText}` : t('project.edited', { lng, project });

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(description)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}