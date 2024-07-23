
import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, WeightedStatusEntry } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetObserverAlias } from "../actions/getalias.action";
import { AirDate } from "../actions/airdate.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { t } from "i18next";

export const BlameCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, locale: lng } = interaction;
  
  await interaction.deferReply();
  
  const { guildId, project } = await GetObserverAlias(db, dbdata, interaction, options.getString('project')!);
  let selGuildId = guildId;
  
  let episode: number | null = options.getNumber('episode');
  let explain: boolean | null = options.getBoolean('explain');

  let epvalue;
  if (selGuildId == null || !(selGuildId in dbdata.guilds))
    return fail(t('error.noSuchProject', { lng }), interaction);

  let projects = dbdata.guilds[selGuildId];
  if (!project || !(project in projects))
    return fail(t('error.noSuchProject', { lng }), interaction);

  let status = '';
  let entries: {[key:string]:WeightedStatusEntry} = {};
  let success = false;
  let started = false;
  let updateTime = 0;
  for (let ep in projects[project].episodes) {
    let projObj = projects[project].episodes[ep];
    if ((episode != null && projObj.number === episode) || (episode == null && projObj.done == false)) {
      success = true;
      episode = projObj.number;
      if (projObj.updated && projObj.updated !== 0) updateTime = projObj.updated;
      entries = GenerateEntries(dbdata, selGuildId, project, episode);
      let map: {[key:string]:string} = {};
      if (explain != null && explain == true) {
        if (projects[project].keyStaff) Object.values(projects[project].keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
        if (projObj.additionalStaff) Object.values(projObj.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
      }
      for (let task in projects[project].episodes[ep].tasks) {
        let taskObj = projects[project].episodes[ep].tasks[task];
        if (taskObj.done) entries[taskObj.abbreviation].status = `~~${taskObj.abbreviation}~~`;
        else entries[taskObj.abbreviation].status = `**${taskObj.abbreviation}**`;

        if (explain != null && explain == true) {
          let title = (taskObj.abbreviation in map) ? map[taskObj.abbreviation] : 'Unknown';
          entries[taskObj.abbreviation].status += `: ${title}${taskObj.done ? ' *(done)*' : ''}`;
        }
        if (taskObj.done) started = true;
      }
    }
  }

  if (!success)
    return fail(t('error.blameFailureGeneric', { lng }), interaction);

  if (explain != null && explain == true)
    status = EntriesToStatusString(entries, '\n');
  else 
    status = EntriesToStatusString(entries);

  if (explain && projects[project].aliases && projects[project].aliases.length > 0)
    status += `\n\nAliases: ${projects[project].aliases.toString()}`;
  else status += '\n';

  if (projects[project].anidb && episode != null && !started)
    status += `\n${await AirDate(projects[project].anidb, projects[project].airTime, episode, dbdata, lng)}`;
  else if (updateTime !== 0)
    status += `\n${t('episode.lastUpdated', { lng, rel: `<t:${updateTime}:R>` })}`;

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(t('title.blamedEpisode', { lng, episode }))
    .setThumbnail(projects[project].poster)
    .setDescription(status)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

}