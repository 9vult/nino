
import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, WeightedStatusEntry } from "../misc/types";
import { Database } from "@firebase/database-types";
import { fail } from "../actions/fail.action";
import { GetAlias } from "../actions/getalias.action";
import { EntriesToStatusString, GenerateEntries } from "../actions/generateEntries.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";

export const RosterCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, locale, user, guildId } = interaction;
  
  await interaction.deferReply();
  
  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  
  let episode: number = options.getNumber('episode')!;

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(GetStr(dbdata.i18n, 'noSuchProject', locale), interaction);

  let projects = dbdata.guilds[guildId];
  if (!project || !(project in projects))
    return fail(GetStr(dbdata.i18n, 'noSuchProject', locale), interaction);

  if (!projects[project].keyStaff) return fail(GetStr(dbdata.i18n, 'noRoster', locale), interaction);
  
  // Check if the user is a Key Staff
  if (projects[project].owner !== user!.id
    && !Object.values(projects[project].keyStaff).map((s) => s.id).includes(user!.id))
      return await fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

  let status = '';
  let entries: {[key:string]:WeightedStatusEntry} = {};
  let success = false;
  for (let ep in projects[project].episodes) {
    let projObj = projects[project].episodes[ep];
    if ((projObj.number === episode)) {
      success = true;
      episode = projObj.number;
      entries = GenerateEntries(dbdata, guildId, project, episode);
      let map: {[key:string]:string} = {};
      if (projects[project].keyStaff) Object.values(projects[project].keyStaff).forEach(ks => { map[ks.role.abbreviation] = ks.id; });
      if (projObj.additionalStaff) Object.values(projObj.additionalStaff).forEach(as => { map[as.role.abbreviation] = as.id });

      for (let task in projects[project].episodes[ep].tasks) {
        let taskObj = projects[project].episodes[ep].tasks[task];
        if (taskObj.done) entries[taskObj.abbreviation].status = `~~${taskObj.abbreviation}~~`;
        else entries[taskObj.abbreviation].status = `**${taskObj.abbreviation}**`;
        
        let staffMember = (taskObj.abbreviation in map) ? `<@${map[taskObj.abbreviation]}>` : 'Unknown';
        entries[taskObj.abbreviation].status += `: ${staffMember}`;
      }
    }
  }

  if (!success)
    return fail(GetStr(dbdata.i18n, 'blameFailure', locale), interaction);

  status = EntriesToStatusString(entries, '\n');

  const embed = new EmbedBuilder()
    .setAuthor({ name: `${projects[project].title} (${projects[project].type})` })
    .setTitle(interp(GetStr(dbdata.i18n, 'blame', interaction.locale), { '$EPISODE': episode }))
    .setThumbnail(projects[project].poster)
    .setDescription(status)
    .setColor(0xd797ff)
    .setTimestamp(Date.now());
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });

}