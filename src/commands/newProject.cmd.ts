import { CacheType, Client, EmbedBuilder, Interaction } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { Database } from "@firebase/database-types";

export const NewProjectCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: Interaction<CacheType>) => {
  if (!interaction.isCommand()) return;
  const { commandName, options, user, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const nickname = String(options.get('nickname')!.value!);
  const title = String(options.get('title')!.value!);
  const owner = String(user!.id);
  const type = String(options.get('type')!.value!);
  const length = Number(options.get('length')!.value!);
  const poster = String(options.get('poster')!.value!);
  const updateChannel = String(options.get('updatechannel')!.value!);
  const releaseChannel = String(options.get('releasechannel')!.value!);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${nickname}`);
  const newProj: Project = {
    nickname,
    title,
    owner,
    length,
    poster,
    type,
    keyStaff: [],
    episodes: [],
    done: false,
    updateChannel,
    releaseChannel
  };
  ref.set(newProj);

  let epref = ref.child('episodes');
  for (let i = 1; i < length + 1; i++) {
    epref.push({
      number: i,
      done: false,
      additionalStaff: [],
      tasks: []
    });
  }

  const embed = new EmbedBuilder()
    .setTitle(`Project Creation`)
    .setDescription(`Created project ${nickname}.\nRemember to add staff/positions!`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}