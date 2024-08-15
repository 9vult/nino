import { ChannelType, ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { AlertError } from "../actions/alertError";
import { nonce } from "../actions/nonce";

export const ReleaseCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { guildId, options, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  let type = options.getString('type')!;
  let number = options.getString('number')!;
  const url: string | null = options.getString('url');
  const role = options.getRole('role');

  if (type == 'Custom') type = '';
  else type = `${type} `;

  if (!isNaN(Number(number))) {
    number = `${Number(number)}`;
  }
  
  let publishNumber = type !== 'Batch' ? number : `(${number})`;
  let publishRole = role !== null ? ( role.name == "@everyone" ? `@everyone ` : `<@&${role.id}> ` ) : '';

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);

  const project = projects[projectName];

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(t('title.episodeReleased', { lng }))
    .setDescription(t('progress.released', { lng, title: project.title, type, publishNumber }))
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  const prefix = dbdata.configuration[guildId!]?.releasePrefix ? `${dbdata.configuration[guildId!]?.releasePrefix} ` : '';

  const publishBody = `${prefix}**${project.title} - ${type}${publishNumber}**\n${publishRole}${url}`;
  const publishChannel = client.channels.cache.get(project.releaseChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ content: publishBody, ...nonce() })
    .then((msg) => {
      if (msg.channel.type === ChannelType.GuildAnnouncement)
      msg.crosspost().catch(console.error);
    })
    .catch(err => AlertError(client, err, guildId!, project.nickname, project.owner, 'Release'));
  }


  if (!project.observers) return; // Stop here if there's no observers
  for (let observerid in project.observers) {
    const observer = project.observers[observerid];
    if (!observer.releasesWebhook) continue;

    let observerPublishRole = (observer.releaseRole && observer.releaseRole != '')
      ? ( observer.releaseRole == "@everyone" ? `@everyone ` : `<@&${observer.releaseRole}> ` )
      : '';

    try {
      const postUrl = new URL(observer.releasesWebhook);
      fetch(postUrl, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({
          username: 'Nino',
          avatar_url: 'https://i.imgur.com/PWtteaY.png',
          content: `${prefix}**${project.title} - ${type}${publishNumber}**\n${observerPublishRole}${url}`,
        })
      });
    } catch {
      interaction.channel?.send({
        content: `Webhook ${observer.releasesWebhook} from ${observer.guildId} failed.`,
        ...nonce()
      });
    }
  }
}