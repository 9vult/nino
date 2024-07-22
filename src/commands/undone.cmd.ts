import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const UndoneCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const selectedEpisode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let episodeId;
  let taskvalue;
  let taskName;
  let isValidUser = false;

  let verification = await VerifyInteraction(dbdata, interaction, alias, false);
  if (!verification) return;
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);

  let localStatus = '';
  let publicStatus = '';
  let localEntries = GenerateEntries(dbdata, guildId!, projectName, selectedEpisode);
  let publicEntries = GenerateEntries(dbdata, guildId!, projectName, selectedEpisode);

  let extended = dbdata.configuration[guildId!]?.progressDisplay == 'Extended';

  let project = projects[projectName];

  for (let keyStaffId in project.keyStaff) {
    let keyStaff = project.keyStaff[keyStaffId];
    if (keyStaff.role.abbreviation === abbreviation && (
      keyStaff.id === user.id || 
      project.owner === user.id || 
      project.administrators?.includes(user.id) ||
      dbdata.configuration[guildId!]?.administrators?.includes(user.id)
    )) {
      isValidUser = true;
      taskName = keyStaff.role.title;
      localStatus = `❌ **${keyStaff.role.title}**\n`;
    }
  }

  for (let epId in project.episodes) {
    const episode = project.episodes[epId];
    if (episode.number == selectedEpisode) {
      episodeId = epId;

      let map: {[key:string]:string} = {};
      if (extended) {
        publicEntries = GenerateEntries(dbdata, guildId!, projectName, episode.number);
        if (project.keyStaff) Object.values(project.keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
        if (episode.additionalStaff)
          Object.values(episode.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
      }

      for (let taskId in episode.tasks) {
        let task = episode.tasks[taskId];
        if (task.abbreviation === abbreviation) {
          taskvalue = taskId;
          if (!task.done)
            return fail(t('taskNotDone', { lng, abbreviation }), interaction);
        }
        // Status string
        let stat = '';
        if (task.abbreviation === abbreviation) stat = `__${abbreviation}__`;
        else if (task.done) stat = `~~${task.abbreviation}~~`;
        else stat = `**${task.abbreviation}**`;

        localEntries[task.abbreviation].status = stat;

        if (extended) {
          let title = (task.abbreviation in map) ? map[task.abbreviation] : 'Unknown';
          let suffix = task.abbreviation == abbreviation ? ' (incomplete)' : task.done ? ' *(done)*' : ''
          publicEntries![task.abbreviation].status = `${stat}: ${title}${suffix}`;
        }
      }

      publicStatus += localStatus;
      if (extended) publicStatus += EntriesToStatusString(publicEntries!, '\n');
      localStatus += EntriesToStatusString(localEntries);

      if (taskvalue == undefined) return fail(t('noSuchTask', { lng, abbreviation }), interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaffId in episode.additionalStaff) {
          let addStaff = episode.additionalStaff[addStaffId];
          if (addStaff.role.abbreviation === abbreviation && (
              addStaff.id === user.id ||
              project.owner === user.id ||
              project.administrators?.includes(user.id) ||
              dbdata.configuration[guildId!]?.administrators?.includes(user.id)
          )) {
            localStatus = `❌ **${addStaff.role.title}**\n` + localStatus;
            publicStatus = `❌ **${addStaff.role.title}**\n` + publicStatus;
            taskName = addStaff.role.title;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail(t('permissionDenied', { lng }), interaction);

  if (taskvalue != undefined) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId}/tasks/${taskvalue}`).update({
      abbreviation, done: false
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId!}`).update({
      updated: utc
    });
  }

  db.ref(`/Projects/${guildId}/${projectName}/episodes/${episodeId}`).update({ done: false });

  const succinctBody = `${t('taskIncompleteBody', { lng, taskName, episode: selectedEpisode })}`
  const verboseBody = `${succinctBody}\n${EntriesToStatusString(localEntries)}`;
  const succinctTitle = `❌ ${t('taskIncompleteTitle', { lng })}`;
  const verboseTitle = `❌ Episode ${selectedEpisode}`;
  const useVerbose = dbdata.configuration[guildId!]?.doneDisplay === 'Verbose';

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(useVerbose ? verboseTitle : succinctTitle)
    .setDescription(useVerbose ? verboseBody : succinctBody)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(`Episode ${selectedEpisode}`)
    .setThumbnail(project.poster)
    .setDescription(!extended ? localStatus : publicStatus)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(project.updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(`[Undone]: "${err.message}" from guild ${guildId}, project ${project.nickname}`));
  }

  if (!project.observers) return; // Stop here if there's no observers
    for (let observerid in project.observers) {
      const observer = project.observers[observerid];
      if (!observer.updatesWebhook) continue;
      try {
        const postUrl = new URL(observer.updatesWebhook);
        fetch(postUrl, {
          method: 'POST',
          headers: { 'content-type': 'application/json' },
          body: JSON.stringify({
            username: 'Nino',
            avatar_url: 'https://i.imgur.com/PWtteaY.png',
            content: '',
            embeds: [ publishEmbed.toJSON() ]
          })
        });
      } catch {
        interaction.channel?.send(`Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`);
      }
    }
}