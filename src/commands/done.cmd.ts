import { ActionRowBuilder, ButtonBuilder, ButtonStyle, ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Episode, Task, WeightedStatusEntry } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

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
  let localEntries: { [key: string]: WeightedStatusEntry };
  let publicEntries: { [key: string]: WeightedStatusEntry };

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
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  let extended = dbdata.configuration && dbdata.configuration[guildId!] && dbdata.configuration[guildId!].progressDisplay && dbdata.configuration[guildId!].progressDisplay == 'Extended';

  // Find selected episode or current working episode
  let success = false;
  for (let ep in projects[project].episodes) {
    let projObj = projects[project].episodes[ep];
    if ((selectedEpisode != null && projObj.number == selectedEpisode) || (selectedEpisode == null && projObj.done == false)) {
      if (!workingEpisode) {
        workingEpisode = projObj;  // Only assign the first undone episode
        workingEpisodeKey = ep;
      }
      if (selectedEpisode != null) {  // Skip further search if the episode is specified (get the task, though)
        nextEpisode = workingEpisode;
        nextEpisodeKey = ep;
        success = true;
        break;
      }
    }
    // See if the task is done at this episode
    for (let task in projObj.tasks) {
      let taskObj = projObj.tasks[task];
      if (taskObj.abbreviation === abbreviation && !taskObj.done) {
        nextEpisode = projObj;
        nextEpisodeTask = taskObj;
        nextEpisodeKey = ep;
        nextEpisodeTaskKey = task;
        success = true;
        break;
      }
    }
    if (success) break;
  }
  if (!success || !nextEpisode || !workingEpisode)
    return fail(t('doneFailure', { lng }), interaction);

  // Get the status of the task at the working episode
  for (let task in workingEpisode.tasks) {
    let taskObj = workingEpisode.tasks[task];
    if (taskObj.abbreviation === abbreviation) {
      workingEpisodeTask = taskObj;
      workingEpisodeTaskKey = task;
      break;
    }
  }

  // Verify the user has permission to proceed
  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.role.abbreviation === abbreviation && (staffObj.id === user.id || projects[project].owner === user.id)) {
      isValidUser = true;
      taskName = staffObj.role.title;
      localStatus = `✅ **${staffObj.role.title}**\n`;
    }
  }
  if (!isValidUser) { // Not key staff
    for (let addStaff in workingEpisode.additionalStaff) {
      let addStaffObj = workingEpisode.additionalStaff[addStaff];
      if (addStaffObj.role.abbreviation === abbreviation && (addStaffObj.id === user.id || projects[project].owner === user.id)) {
        localStatus = `✅ **${addStaffObj.role.title}**\n`;
        taskName = addStaffObj.role.title;
        isValidUser = true;
        isAdditionalStaff = true;
      }
    }
  }

  if (!isValidUser)
    return fail(t('permissionDenied', { lng }), interaction);

  // User is valid (yippee!) so proceed
  // OPTION ONE
  // Episode number is SPECIFIED and Task is NOT DONE
  // OPTION TWO
  // Episode number is UNSPECIFIED, Working Episode == Next Episode

  if ((!postable && selectedEpisode != null && !workingEpisodeTask?.done)
    || (selectedEpisode == null && workingEpisode == nextEpisode)) {
    localEntries = GenerateEntries(dbdata, guildId!, project, workingEpisode.number);

    let map: {[key:string]:string} = {};
    if (extended) {
      publicEntries = GenerateEntries(dbdata, guildId!, project, workingEpisode.number);
      if (projects[project].keyStaff) Object.values(projects[project].keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
      if (nextEpisode.additionalStaff) Object.values(nextEpisode.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
    }

    for (let task in workingEpisode.tasks) {
      let taskObj = workingEpisode.tasks[task];
      if (taskObj.abbreviation === abbreviation) {
        if (taskObj.done)
          return fail(t('taskAlreadyDone', {lng, abbreviation}), interaction);
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
    db.ref(`/Projects/${guildId}/${project}/episodes/${workingEpisodeKey!}/tasks/${workingEpisodeTaskKey!}`).update({
      abbreviation, done: true
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${project}/episodes/${nextEpisodeKey!}`).update({
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
    return fail(t('taskAlreadyDone', {lng, abbreviation}), interaction);
  }

  // OPTION FIVE
  // Episode number is UNSPECIFIED, Working Episode != Next Episode (NOT ADDITIONAL STAFF)
  if (!postable && selectedEpisode == null && workingEpisode != nextEpisode) {
    const msgBody = t('inTheDust', { lng, currentEpisode: workingEpisode.number, taskName, nextEpisode: nextEpisode.number });
    const proceed = new ButtonBuilder()
      .setCustomId('ninodoneproceed')
      .setLabel(t('proceed', { lng }))
      .setStyle(ButtonStyle.Danger);
    const cancel = new ButtonBuilder()
      .setCustomId('ninodonecancel')
      .setLabel(t('cancel', { lng }))
      .setStyle(ButtonStyle.Secondary);
    const replyEmbed = new EmbedBuilder()
      .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
      .setTitle(`❓ ${t('choice', { lng })}`)
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
        editBody = t('dontDoIt', { lng });
      }
      else if (confirmation.customId === 'ninodoneproceed') {
        replied = true;
        let diff = Math.ceil(nextEpisode.number - workingEpisode.number);
        editBody = t('doItNow', { lng, taskName, count: diff });

        localEntries = GenerateEntries(dbdata, guildId!, project, nextEpisode.number);
        
        let map: {[key:string]:string} = {};
        if (extended) {
          publicEntries = GenerateEntries(dbdata, guildId!, project, workingEpisode.number);
          if (projects[project].keyStaff) Object.values(projects[project].keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
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
        db.ref(`/Projects/${guildId}/${project}/episodes/${nextEpisodeKey!}/tasks/${nextEpisodeTaskKey!}`).update({
          abbreviation, done: true
        });
        const utc = Math.floor(new Date().getTime() / 1000);
        db.ref(`/Projects/${guildId}/${project}/episodes/${nextEpisodeKey!}`).update({
          updated: utc
        });
        postable = true;
      }
    } catch (e) {
      editBody = t('noResponse', { lng });
    }
    const editedEmbed = new EmbedBuilder()
      .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
      .setTitle(`❓ ${t('choice', { lng })}`)
      .setDescription(editBody ?? 'i18n failed')
      .setColor(0xd797ff)
      .setTimestamp(Date.now());
    await interaction.editReply({ embeds: [editedEmbed], components: [] });
  }

  if (!postable) return;

  if (episodeDone) {
    db.ref(`/Projects/${guildId}/${project}/episodes/${nextEpisodeKey!}`).update({ done: true });
  }

  const episodeDoneText = episodeDone ? `\n${t('episodeDone', { lng, episode: nextEpisode.number })}` : '';
  const replyEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`✅ ${t('taskCompleteTitle', { lng })}`)
    .setDescription(`${t('taskCompleteBody', { lng, taskName, episode: nextEpisode.number })}${episodeDoneText}`)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  if (!replied)
    await interaction.editReply({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });
  else
    await interaction.followUp({ embeds: [replyEmbed], allowedMentions: generateAllowedMentions([[], []]) });

  // If the episode is done, check if the project as a whole is complete
  if (episodeDone) {
    let projectComplete = true;
    for (let ep in projects[project].episodes) {
      let projObj = projects[project].episodes[ep];
      if (projObj.number != workingEpisode.number && !projObj.done) {
        projectComplete = false;
        break;
      }
    }    
    if (projectComplete) {
      db.ref(`/Projects/${guildId}/${project}`).update({ done: true });
      const completeEmbed = new EmbedBuilder()
        .setTitle(`${t('youDidItTitle', { lng })}`)
        .setDescription(`${t('youDidItBody', { lng, title: projects[project].title })}`)
        .setColor(0xd797ff)
        .setTimestamp(Date.now());
      await interaction.channel?.send({ embeds: [completeEmbed], allowedMentions: generateAllowedMentions([[], []]) });
    }
  }

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`Episode ${nextEpisode.number}`)
    .setThumbnail(projects[project].poster)
    .setDescription(!extended ? localStatus : publicStatus)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(projects[project].updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(err));
  }

  if (!projects[project].observers) return; // Stop here if there's no observers
  for (let observerid in projects[project].observers) {
    const observer = projects[project].observers[observerid];
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
      interaction.channel?.send(`Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`);
    }
  }

}