import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";

export const AddObserverCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const observingGuild = options.getString('guild')!;
  const updatesWH: string | null = options.getString('updates');
  const relesesWH: string | null = options.getString('releases');

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(`Guild ${guildId} does not exist.`, interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(`Project ${project} does not exist.`, interaction);
  if (projects[project].owner !== user!.id)
    return fail(`You do not have permission to do that.`, interaction);

  db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`).child('observers')
    .push({ guildId: observingGuild, updatesWebhook: updatesWH, releasesWebhook: relesesWH });

  const ref = db.ref(`/Observers`).child(`${observingGuild}`);
  if (dbdata.observers 
      && dbdata.observers[observingGuild] 
      && dbdata.observers[observingGuild][guildId]
    ) {
      let data: {[key:string]:string[]} = {};
      data[guildId] = [...dbdata.observers[observingGuild][guildId], project];
      ref.update(data);
    }
  else {
    let data: {[key:string]:string[]} = {};
    data[guildId] = [project];
    ref.update(data)
  }

  const embed = new EmbedBuilder()
    .setTitle(`Project Modification`)
    .setDescription(`I added the observer ${observingGuild} to \`${project}\` for you.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}