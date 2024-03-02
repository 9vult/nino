
import { CacheType, Client, EmbedBuilder, Interaction } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { Database } from "@firebase/database-types";

export const AddAdditionalStaffCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: Interaction<CacheType>) => {
  if (!interaction.isCommand()) return;
  const { commandName, options, user, member } = interaction;

  await interaction.deferReply();

  const project = String(options.get('project')!.value!);
  const episode = Number(options.get('episode')!.value!);
  const staff = String(options.get('member')!.value!);
  const abbreviation = String(options.get('abbreviation')!.value!).toUpperCase();
  const title = String(options.get('title')!.value!);

  let epvalue;
  let faildesc;
  if (!(project in dbdata.projects))
    faildesc = `Project ${project} does not exist.`;
  if (dbdata.projects[project].owner !== user!.id)
    faildesc = `You do not have permission to do that.`;
  for (let ep in dbdata.projects[project].episodes)
    if (dbdata.projects[project].episodes[ep].number == episode) {
      epvalue = ep;
      for (let pos in dbdata.projects[project].episodes[ep].additionalStaff)
        if (dbdata.projects[project].episodes[ep].additionalStaff[pos].role.abbreviation == abbreviation) {
          faildesc = `That position already exists.`;
          break;
        }
    }

  if (faildesc !== undefined) {
    const embed = new EmbedBuilder()
      .setTitle(`Project Creation`)
      .setDescription(faildesc)
      .setColor(0xd797ff);
    await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
    return;
  }

  db.ref(`/Projects/${project}/episodes/${epvalue}`).child("additionalStaff").push({
    id: staff,
    role: {
      abbreviation,
      title
    }
  });

  db.ref(`/Projects/${project}/episodes/${epvalue}`).child("tasks").push({
    abbreviation, done: false
  });


  const embed = new EmbedBuilder()
    .setTitle(`Project Creation`)
    .setDescription(`Added <@${staff}> for position ${abbreviation}.`)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}