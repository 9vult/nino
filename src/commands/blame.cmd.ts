
import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, WeightedStatusEntry } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetObserverAlias } from "../actions/getalias.action";
import { AirDate } from "../actions/airdate.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { t } from "i18next";
import { getBlameableEpisode } from "../actions/getters";

export const BlameCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, locale: lng } = interaction;
  
  await interaction.deferReply();
  
  const { guildId, project: projectName } = await GetObserverAlias(db, dbdata, interaction, options.getString('project')!);
  let selGuildId = guildId;
  
  let episodeNumber: number | null = options.getNumber('episode');
  let explain: boolean | null = options.getBoolean('explain');

  let epvalue;
  if (selGuildId == null || !(selGuildId in dbdata.guilds))
    return fail(t('error.noSuchProject', { lng }), interaction);

  let projects = dbdata.guilds[selGuildId];
  if (!projectName || !(projectName in projects))
    return fail(t('error.noSuchProject', { lng }), interaction);

  let project = projects[projectName];

  let status = '';
  let entries: {[key:string]:WeightedStatusEntry} = {};
  let success = false;
  let started = false;
  let updateTime = 0;


  let { episode } = getBlameableEpisode(project, episodeNumber);
  if (!episode) return fail(t('error.blameFailureGeneric', { lng }), interaction);

  episodeNumber = episode.number;
  if (episode.updated && episode.updated !== 0) updateTime = episode.updated;
  entries = GenerateEntries(dbdata, selGuildId, projectName, episodeNumber);
  let map: {[key:string]:string} = {};
  if (explain != null && explain == true) {
    if (project.keyStaff) Object.values(project.keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
    if (episode.additionalStaff) Object.values(episode.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
  }
  for (let task in episode.tasks) {
    let taskObj = episode.tasks[task];
    if (taskObj.done) entries[taskObj.abbreviation].status = `~~${taskObj.abbreviation}~~`;
    else entries[taskObj.abbreviation].status = `**${taskObj.abbreviation}**`;

    if (explain != null && explain == true) {
      let title = (taskObj.abbreviation in map) ? map[taskObj.abbreviation] : 'Unknown';
      entries[taskObj.abbreviation].status += `: ${title}${taskObj.done ? ' *(done)*' : ''}`;
    }
    if (taskObj.done) started = true;
  }


  if (explain != null && explain == true)
    status = EntriesToStatusString(entries, '\n');
  else 
    status = EntriesToStatusString(entries);

  if (explain && project.aliases && project.aliases.length > 0)
    status += `\n\nAliases: ${project.aliases.toString()}`;
  else status += '\n';

  if (project.anidb && episodeNumber != null && !started)
    status += `\n${await AirDate(project.anidb, project.airTime, episodeNumber, dbdata, lng)}`;
  else if (updateTime !== 0)
    status += `\n${t('episode.lastUpdated', { lng, rel: `<t:${updateTime}:R>` })}`;

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(t('title.blamedEpisode', { lng, episode: episodeNumber }))
    .setThumbnail(project.poster)
    .setDescription(status)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

}