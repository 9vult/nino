import { ChannelType, ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel, WebhookClient } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";

export const ReleaseCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, locale } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const type = options.getString('type')!;
  const number = options.getString('number')!;
  const url: string | null = options.getString('url');
  const role = options.getRole('role');
  
  let publishNumber = type !== 'Batch' ? number : `(${number})`;
  let publishRole = role !== null ? `<@&${role.id}> ` : '';

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(GetStr(dbdata.i18n, 'episodeReleasedTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'episodeReleasedBody', interaction.locale), { '$TITLE': projects[project].title, '$TYPE': type, '$PUBLISHNUMBER': publishNumber }))
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishBody = `**${projects[project].title} - ${type} ${publishNumber}**\n${publishRole}${url}`;
  const publishChannel = client.channels.cache.get(projects[project].releaseChannel);
  if (publishChannel?.isTextBased)
    (publishChannel as TextChannel).send(publishBody)
    .then((msg) => {
      if (msg.channel.type === ChannelType.GuildAnnouncement)
        msg.crosspost().catch(console.error);
    });

  if (!projects[project].observers) return; // Stop here if there's no observers
  for (let observerid in projects[project].observers) {
    const observer = projects[project].observers[observerid];
    if (!observer.releasesWebhook) continue;
    try {
      fetch(observer.releasesWebhook, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({
          username: 'Nino',
          avatar_url: 'https://i.imgur.com/PWtteaY.png',
          content: `**${projects[project].title} - ${type} ${publishNumber}**\n${url}`,
        })
      });
    } catch {
      interaction.channel?.send(`Webhook ${observer.releasesWebhook} failed.`);
    }
  }
}