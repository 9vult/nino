import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";

export const ReleaseCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId } = interaction;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const type = options.getString('type')!;
  const number = options.getString('number')!;
  const url: string | null = options.getString('url');
  const role = options.getRole('role')!.id;
  
  if (guildId == null || !(guildId in dbdata.guilds))
  return fail(`Guild ${guildId} does not exist.`, interaction);

let projects = dbdata.guilds[guildId];
let publishNumber = type !== 'Batch' ? number : `(${number})`;
let publishRole = role !== null ? `<@&${role}> ` : '';

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user.id)
    return fail('You do not have permission to do that.', interaction);

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`Episode Released`)
    .setDescription(`Nice job releasing **${projects[project].title} ${type} ${publishNumber}**!\nI'm proud of you!`)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishBody = `**${projects[project].title} - ${type} ${publishNumber}**\n${publishRole}${url}`;
  const publishChannel = client.channels.cache.get(projects[project].releaseChannel);
  if (publishChannel?.isTextBased)
    (publishChannel as TextChannel).send(publishBody);
}