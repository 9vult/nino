import { ChatInputCommandInteraction, Client, EmbedBuilder, TextChannel } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";

export const UndoneCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, guildId, locale } = interaction;

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

  let status = '';
  let entries = GenerateEntries(dbdata, guildId!, project, episode);

  for (let staff in projects[project].keyStaff) {
    let staffObj = projects[project].keyStaff[staff];
    if (staffObj.role.abbreviation === abbreviation && (staffObj.id === user.id || projects[project].owner === user.id)) {
      isValidUser = true;
      taskName = staffObj.role.title;
      status = `❌ **${staffObj.role.title}**\n`;
    }
  }

  for (let ep in projects[project].episodes) {
    if (projects[project].episodes[ep].number == episode) {
      epvalue = ep;
      for (let task in projects[project].episodes[ep].tasks) {
        let taskObj = projects[project].episodes[ep].tasks[task];
        if (taskObj.abbreviation === abbreviation) {
          taskvalue = task;
          if (!taskObj.done)
            return fail(interp(GetStr(dbdata.i18n, 'taskNotDone', interaction.locale), { '$ABBREVIATION': abbreviation }), interaction);
        }
        // Status string
        if (taskObj.abbreviation === abbreviation) entries[taskObj.abbreviation].status = `__${abbreviation}__`;
        else if (taskObj.done) entries[taskObj.abbreviation].status = `~~${taskObj.abbreviation}~~`;
        else entries[taskObj.abbreviation].status = `**${taskObj.abbreviation}**`;
      }

      status += EntriesToStatusString(entries);

      if (taskvalue == undefined) return fail(interp(GetStr(dbdata.i18n, 'noSuchTask', interaction.locale), { '$ABBREVIATION': abbreviation }), interaction);
      if (!isValidUser) { // Not key staff
        for (let addStaff in projects[project].episodes[ep].additionalStaff) {
          let addStaffObj = projects[project].episodes[ep].additionalStaff[addStaff];
          if (addStaffObj.role.abbreviation === abbreviation && (addStaffObj.id === user.id || projects[project].owner === user.id)) {
            status = `❌ **${addStaffObj.role.title}**\n` + status;
            taskName = addStaffObj.role.title;
            isValidUser = true;
          }
        }
      }
    }
  }

  if (!isValidUser)
    return fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

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
    .setTitle(`❌ ${GetStr(dbdata.i18n, 'taskIncompleteTitle', interaction.locale)}`)
    .setDescription(interp(GetStr(dbdata.i18n, 'taskIncompleteBody', interaction.locale), { '$TASKNAME': taskName, '$EPISODE': episode }))
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

  const publishEmbed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(`Episode ${episode}`)
    .setThumbnail(projects[project].poster)
    .setDescription(status)
    .setTimestamp(Date.now());
  const publishChannel = client.channels.cache.get(projects[project].updateChannel);
  if (publishChannel?.isTextBased)
    (publishChannel as TextChannel).send({ embeds: [publishEmbed] })

  if (!projects[project].observers) return; // Stop here if there's no observers
    for (let observerid in projects[project].observers) {
      const observer = projects[project].observers[observerid];
      if (!observer.updatesWebhook) continue;
      try {
        fetch(observer.updatesWebhook, {
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
        interaction.channel?.send(`Webhook ${observer.updatesWebhook} failed.`);
      }
    }
}