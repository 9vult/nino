import { ActionRowBuilder, ButtonBuilder, ButtonStyle, ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Episode, Task, WeightedStatusEntry } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { AlertError } from "../actions/alertError";
import { nonce } from "../actions/nonce";

export const DoneCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale: lng } = interaction;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const abbreviation = options.getString('abbreviation')!.toUpperCase();
  let selectedEpisode: number | null = options.getNumber('episode');

  let localStatus = '';
  let publicStatus = '';
  let taskName;
  let isValidUser = false;
  let isAdditionalStaff = false;
  let episodeDone = true;
  let localEntries: { [key: string]: WeightedStatusEntry } = {};
  let publicEntries: { [key: string]: WeightedStatusEntry } = {};

  let workingEpisode: Episode | undefined;
  let workingEpisodeTask: Task | undefined;
  let workingEpisodeKey: string;
  let workingEpisodeTaskKey: string;
  let nextEpisode: Episode | undefined;
  let nextEpisodeTask: Task | undefined;
  let nextEpisodeKey: string;
  let nextEpisodeTaskKey: string;
  let postable = false;
  let replied = false;

  let verification = await VerifyInteraction(dbdata, interaction, alias, false);
  if (!verification) return;
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);

  let extended = dbdata.configuration[guildId!]?.progressDisplay == 'Extended';

  let success = false;
  const project = projects[projectName];
  const lock = project.isPrivate ? '🔒 ' : '';

  // Find selected episode or current working episode
  for (let epId in project.episodes) {
    let episode = project.episodes[epId];
    if ((selectedEpisode != null && episode.number == selectedEpisode) || (selectedEpisode == null && episode.done == false)) {
      if (!workingEpisode) {
        workingEpisode = episode;  // Only assign the first undone episode
        workingEpisodeKey = epId;
      }
      if (selectedEpisode != null) {  // Skip further search if the episode is specified (get the task, though)
        nextEpisode = workingEpisode;
        nextEpisodeKey = epId;
        success = true;
        break;
      }
    } else continue;

    // See if the task is done at this episode
    for (let taskId in episode.tasks) {
      let task = episode.tasks[taskId];
      if (task.abbreviation === abbreviation && !task.done) {
        nextEpisode = episode;
        nextEpisodeTask = task;
        nextEpisodeKey = epId;
        nextEpisodeTaskKey = taskId;
        success = true;
        break;
      }
    }
    if (success) break;
  }
  if (!success || !nextEpisode || !workingEpisode)
    return fail(t('error.doneFailureGeneric', { lng }), interaction);

  // Get the status of the task at the working episode
  for (let taskId in workingEpisode.tasks) {
    let task = workingEpisode.tasks[taskId];
    if (task.abbreviation === abbreviation) {
      workingEpisodeTask = task;
      workingEpisodeTaskKey = taskId;
      break;
    }
  }

  // Verify the user has permission to proceed
  for (let keyStaffId in project.keyStaff) {
    let keyStaff = project.keyStaff[keyStaffId];
    if (keyStaff.role.abbreviation === abbreviation && (
        keyStaff.id === user.id || 
        project.owner === user.id || 
        project?.administrators?.includes(user.id) ||
        dbdata.configuration[guildId!]?.administrators?.includes(user.id)
    )) {
      isValidUser = true;
      taskName = keyStaff.role.title;
      localStatus = `✅ **${keyStaff.role.title}**\n`;
    }
  }
  if (!isValidUser) { // Not key staff
    for (let addStaffId in workingEpisode.additionalStaff) {
      let addStaff = workingEpisode.additionalStaff[addStaffId];
      if (addStaff.role.abbreviation === abbreviation && (
          addStaff.id === user.id ||
          project.owner === user.id ||
          project.administrators?.includes(user.id) ||
          dbdata.configuration[guildId!]?.administrators?.includes(user.id)
      )) {
        localStatus = `✅ **${addStaff.role.title}**\n`;
        taskName = addStaff.role.title;
        isValidUser = true;
        isAdditionalStaff = true;
      }
    }
  }

  if (!isValidUser)
    return fail(t('error.permissionDenied', { lng }), interaction);

  // User is valid (yippee!) so proceed
  // OPTION ONE
  // Episode number is SPECIFIED and Task is NOT DONE
  // OPTION TWO
  // Episode number is UNSPECIFIED, Working Episode == Next Episode

  if ((!postable && selectedEpisode != null && !workingEpisodeTask?.done)
    || (selectedEpisode == null && workingEpisode == nextEpisode)) {
    localEntries = GenerateEntries(dbdata, guildId!, projectName, workingEpisode.number);

    let map: {[key:string]:string} = {};
    if (extended) {
      publicEntries = GenerateEntries(dbdata, guildId!, projectName, workingEpisode.number);
      if (project.keyStaff) Object.values(project.keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
      if (nextEpisode.additionalStaff) Object.values(nextEpisode.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
    }

    for (let task in workingEpisode.tasks) {
      let taskObj = workingEpisode.tasks[task];
      if (taskObj.abbreviation === abbreviation) {
        if (taskObj.done)
          return fail(t('error.progress.taskAlreadyDone', {lng, abbreviation}), interaction);
      }
      else if (!taskObj.done) episodeDone = false;
      // Status string
      let stat = '';
      if (taskObj.abbreviation === abbreviation) stat = `__~~${abbreviation}~~__`;
      else if (taskObj.done) stat = `~~${taskObj.abbreviation}~~`;
      else stat = `**${taskObj.abbreviation}**`;
      localEntries[taskObj.abbreviation].status = stat;

      if (extended) {
        let title = (taskObj.abbreviation in map) ? map[taskObj.abbreviation] : 'Unknown';
        publicEntries![taskObj.abbreviation].status = `${stat}: ${title}${taskObj.done ? ' *(done)*' : ''}`;
      }
    }
    publicStatus += localStatus;
    if (extended) publicStatus += EntriesToStatusString(publicEntries!, '\n');
    localStatus += EntriesToStatusString(localEntries);
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${workingEpisodeKey!}/tasks/${workingEpisodeTaskKey!}`).update({
      abbreviation, done: true
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${nextEpisodeKey!}`).update({
      updated: utc
    });
    postable = true;
  }

  // OPTION THREE
  // Episode number is SPECIFIED but Task is ALREADY DONE (FAIL)
  // OPTION FOUR
  // Episode number is UNSPECIFIED, Working Episode != Next Episode *BUT* they are Additional Staff (FAIL)
  if ((!postable && selectedEpisode != null && workingEpisodeTask?.done)
    || (!postable && isAdditionalStaff && selectedEpisode == null && workingEpisode != nextEpisode)) {
    return fail(t('error.progress.taskAlreadyDone', {lng, abbreviation}), interaction);
  }

  // OPTION FIVE
  // Episode number is UNSPECIFIED, Working Episode != Next Episode (NOT ADDITIONAL STAFF)
  if (!postable && selectedEpisode == null && workingEpisode != nextEpisode) {
    const msgBody = t('progress.done.inTheDust', { lng, currentEpisode: workingEpisode.number, taskName, nextEpisode: nextEpisode.number });
    const proceed = new ButtonBuilder()
      .setCustomId('ninodoneproceed')
      .setLabel(t('progress.done.inTheDust.doItNow.button', { lng }))
      .setStyle(ButtonStyle.Danger);
    const cancel = new ButtonBuilder()
      .setCustomId('ninodonecancel')
      .setLabel(t('progress.done.inTheDust.dontDoIt.button', { lng }))
      .setStyle(ButtonStyle.Secondary);
    const replyEmbed = new EmbedBuilder()
      .setAuthor({ name: `${lock}${project.title} (${project.type})` })
      .setTitle(`❓ ${t('progress.done.inTheDust.question', { lng })}`)
      .setDescription(msgBody)
      .setColor(0xd797ff)
      .setTimestamp(Date.now());
      const row = new ActionRowBuilder<ButtonBuilder>().addComponents(proceed, cancel);
      const btnResponse = await interaction.editReply({
        embeds: [replyEmbed],
        components: [row]
      });

    const collectorFilter = (i: any) => i.user.id === interaction.user.id;
    let editBody;
    try {
      const confirmation = await btnResponse.awaitMessageComponent({ filter: collectorFilter, time: 60_000 });
      if (confirmation.customId === 'ninodonecancel') {
        editBody = t('progress.done.inTheDust.dontDoIt', { lng });
      }
      else if (confirmation.customId === 'ninodoneproceed') {
        replied = true;
        let diff = Math.ceil(nextEpisode.number - workingEpisode.number);
        editBody = t('progress.done.inTheDust.doItNow', { lng, taskName, count: diff });

        localEntries = GenerateEntries(dbdata, guildId!, projectName, nextEpisode.number);
        
        let map: {[key:string]:string} = {};
        if (extended) {
          publicEntries = GenerateEntries(dbdata, guildId!, projectName, workingEpisode.number);
          if (project.keyStaff) Object.values(project.keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
          if (nextEpisode.additionalStaff) Object.values(nextEpisode.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
        }

        for (let task in nextEpisode.tasks) {
          let taskObj = nextEpisode.tasks[task];
          // Status string
          let stat = '';
          if (taskObj.abbreviation === abbreviation) stat = `__~~${abbreviation}~~__`;
          else if (taskObj.done) stat = `~~${taskObj.abbreviation}~~`;
          else stat = `**${taskObj.abbreviation}**`;
          localEntries[taskObj.abbreviation].status = stat;

          if (extended) {
            let title = (taskObj.abbreviation in map) ? map[taskObj.abbreviation] : 'Unknown';
            publicEntries![taskObj.abbreviation].status = `${stat}: ${title}${taskObj.done ? ' *(done)*' : ''}`;
          }

          if (!taskObj.done) episodeDone = false;
        }
        publicStatus += localStatus;
        if (extended) publicStatus += EntriesToStatusString(publicEntries!, '\n');
        localStatus += EntriesToStatusString(localEntries);
        db.ref(`/Projects/${guildId}/${projectName}/episodes/${nextEpisodeKey!}/tasks/${nextEpisodeTaskKey!}`).update({
          abbreviation, done: true
        });
        const utc = Math.floor(new Date().getTime() / 1000);
        db.ref(`/Projects/${guildId}/${projectName}/episodes/${nextEpisodeKey!}`).update({
          updated: utc
        });
        postable = true;
      }
    } catch (e) {
      editBody = t('progress.done.inTheDust.timeout', { lng });
    }
    const editedEmbed = new EmbedBuilder()
      .setAuthor({ name: `${lock}${project.title} (${project.type})` })
      .setTitle(`❓ ${t('progress.done.inTheDust.question', { lng })}`)
      .setDescription(editBody ?? 'i18n failed')
      .setColor(0xd797ff)
      .setTimestamp(Date.now());
    await interaction.editReply({ embeds: [editedEmbed], components: [] });
  }

  if (!postable) return;

  if (episodeDone) {
    db.ref(`/Projects/${guildId}/${projectName}/episodes/${nextEpisodeKey!}`).update({ done: true });
  }

  const episodeDoneText = episodeDone ? `\n${t('progress.episodeComplete', { lng, episode: nextEpisode.number })}` : '';
  
  const succinctBody = `${t('progress.done', { lng, taskName, episode: nextEpisode.number })}${episodeDoneText}`
  const verboseBody = `${t('progress.done', { lng, taskName, episode: nextEpisode.number })}\n\n${EntriesToStatusString(localEntries)}${episodeDoneText}`;
  const useVerbose = dbdata.configuration[guildId!]?.doneDisplay === 'Verbose';

  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${lock}${project.title} (${project.type})` })
    .setTitle(`✅ ${t('title.taskComplete', { lng })}`)
    .setDescription(useVerbose ? verboseBody : succinctBody)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  if (!replied)
    await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });
  else
    await interaction.followUp({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  // If the episode is done, check if the project as a whole is complete
  if (episodeDone) {
    let projectComplete = true;
    for (let ep in project.episodes) {
      let epObj = project.episodes[ep];
      if (epObj.number != workingEpisode.number && !epObj.done) {
        projectComplete = false;
        break;
      }
    }    
    if (projectComplete) {
      db.ref(`/Projects/${guildId}/${projectName}`).update({ done: true });
      const completeEmbed = new EmbedBuilder()
        .setTitle(`${t('title.youDidIt', { lng })}`)
        .setDescription(`${t('progress.projectComplete', { lng, title: project.title })}`)
        .setColor(0xd797ff)
        .setTimestamp(Date.now());
      await interaction.channel?.send({ embeds: [completeEmbed], allowedMentions: generateAllowedMentions([[], []]), ...nonce() });
    }
  }

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${project.title} (${project.type})` })
    .setTitle(`Episode ${nextEpisode.number}`)
    .setThumbnail(project.poster)
    .setDescription(!extended ? localStatus : publicStatus)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(project.updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed], ...nonce() })
    .catch(err => AlertError(client, err, guildId!, project.nickname, 'Done'));
  }

  if (!project.observers) return; // Stop here if there's no observers
  for (let observerid in project.observers) {
    const observer = projects[projectName].observers[observerid];
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
          embeds: [publishEmbed.toJSON()]
        })
      });
    } catch {
      interaction.channel?.send({
        content: `Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`,
        ...nonce()
      });
    }
  }

}