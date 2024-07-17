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
  const episode = options.getNumber('episode')!;
  const abbreviation = options.getString('abbreviation')!.toUpperCase();

  let epvalue;
  let taskvalue;
  let taskName;
  let isValidUser = false;

  let verification = await VerifyInteraction(dbdata, interaction, alias, false);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  let localStatus = '';
  let publicStatus = '';
  let localEntries = GenerateEntries(dbdata, guildId!, project, episode);
  let publicEntries = GenerateEntries(dbdata, guildId!, project, episode);

  let extended = dbdata.configuration && dbdata.configuration[guildId!] && dbdata.configuration[guildId!].progressDisplay && dbdata.configuration[guildId!].progressDisplay == 'Extended';

  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.role.abbreviation === abbreviation && (staffObj.id === user.id || projects[project].owner === user.id)) {
      isValidUser = true;
      taskName = staffObj.role.title;
      localStatus = `❌ **${staffObj.role.title}**\n`;
    }
  }

  for (let ep in projects[project].episodes) {
    if (projects[project].episodes[ep].number == episode) {
      epvalue = ep;

      let map: {[key:string]:string} = {};
      if (extended) {
        publicEntries = GenerateEntries(dbdata, guildId!, project, projects[project].episodes[ep].number);
        if (projects[project].keyStaff) Object.values(projects[project].keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.role.title; });
        if (projects[project].episodes[ep].additionalStaff)
          Object.values(projects[project].episodes[ep].additionalStaff).forEach(as => { map[as.role.abbreviation] = as.role.title });
      }

      for (let task in projects[project].episodes[ep].tasks) {
        let taskObj = projects[project].episodes[ep].tasks[task];
        if (taskObj.abbreviation === abbreviation) {
          taskvalue = task;
          if (!taskObj.done)
            return fail(t('taskNotDone', { lng, abbreviation }), interaction);
        }
        // Status string
        let stat = '';
        if (taskObj.abbreviation === abbreviation) stat = `__${abbreviation}__`;
        else if (taskObj.done) stat = `~~${taskObj.abbreviation}~~`;
        else stat = `**${taskObj.abbreviation}**`;

        localEntries[taskObj.abbreviation].status = stat;

        if (extended) {
          let title = (taskObj.abbreviation in map) ? map[taskObj.abbreviation] : 'Unknown';
          let suffix = taskObj.abbreviation == abbreviation ? ' (incomplete)' : taskObj.done ? ' *(done)*' : ''
          publicEntries![taskObj.abbreviation].status = `${stat}: ${title}${suffix}`;
        }
      }

      publicStatus += localStatus;
      if (extended) publicStatus += EntriesToStatusString(publicEntries!, '\n');
      localStatus += EntriesToStatusString(localEntries);

      if (taskvalue == undefined) return fail(t('noSuchTask', { lng, abbreviation }), interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaff in projects[project].episodes[ep].additionalStaff) {
          let addStaffObj = projects[project].episodes[ep].additionalStaff[addStaff];
          if (addStaffObj.role.abbreviation === abbreviation && (addStaffObj.id === user.id || projects[project].owner === user.id)) {
            localStatus = `❌ **${addStaffObj.role.title}**\n` + localStatus;
            publicStatus = `❌ **${addStaffObj.role.title}**\n` + publicStatus;
            taskName = addStaffObj.role.title;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail(t('permissionDenied', { lng }), interaction);

  if (taskvalue != undefined) {
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}/tasks/${taskvalue}`).update({
      abbreviation, done: false
    });
    const utc = Math.floor(new Date().getTime() / 1000);
    db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue!}`).update({
      updated: utc
    });
  }

  db.ref(`/Projects/${guildId}/${project}/episodes/${epvalue}`).update({ done: false });

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`❌ ${t('taskIncompleteTitle', { lng })}`)
    .setDescription(t('taskIncompleteBody', { lng, taskName, episode }))
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`Episode ${episode}`)
    .setThumbnail(projects[project].poster)
    .setDescription(!extended ? localStatus : publicStatus)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(projects[project].updateChannel);

  if (publishChannel?.isTextBased) {
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })
    .catch(err => console.error(`[Undone]: "${err.message}" from guild ${guildId}, project ${projects[project].nickname}`));
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
            embeds: [ publishEmbed.toJSON() ]
          })
        });
      } catch {
        interaction.channel?.send(`Webhook ${observer.updatesWebhook} from ${observer.guildId} failed.`);
      }
    }
}