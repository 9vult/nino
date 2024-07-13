import { Client, BaseInteraction, AutocompleteInteraction, ChatInputCommandInteraction } from "discord.js";
import type { Database } from '@firebase/database-types';

import { DatabaseData } from "../misc/types";
import { NewProjectCmd } from "../commands/newProject.cmd";
import { AddStaffCmd } from "../commands/addStaff.cmd";
import { AddAdditionalStaffCmd } from "../commands/addAdditionalStaff.cmd";
import { HelpCmd } from "../commands/help.cmd";
import { AboutCmd } from "../commands/about.cmd";
import { DoneCmd } from "../commands/done.cmd";
import { UndoneCmd } from "../commands/undone.cmd";
import { ReleaseCmd } from "../commands/release.cmd";
import { SwapStaffCmd } from "../commands/swapStaff.cmd";
import { SwapAdditionalStaffCmd } from "../commands/swapAdditionalStaff.cmd";
import { TransferOwnershipCmd } from "../commands/transferOwnership.cmd";
import { RemoveStaffCmd } from "../commands/removeStaff.cmd";
import { RemoveAdditionalStaffCmd } from "../commands/removeAdditionalStaff.cmd";
import { DeleteProjectCmd } from "../commands/deleteProject.cmd";
import { EditProjectCmd } from "../commands/editProject.cmd";
import { AddEpisodeCmd } from "../commands/addEpisode.cmd";
import { RemoveEpisodeCmd } from "../commands/removeEpisodeCmd";
import { BlameCmd } from "../commands/blame.cmd";
import { SkipCmd } from "../commands/skip.cmd";
import { AddAliasCmd } from "../commands/addAlias.cmd";
import { RemoveAliasCmd } from "../commands/removeAlias.cmd";
import { GetAlias } from "../actions/getalias.action";
import { SetWeightCmd } from "../commands/setWeight.cmd";
import { AddObserverCmd } from "../commands/addObserver.cmd";
import { RemoveObserverCmd } from "../commands/removeObserver.cmd";
import { RosterCmd } from "../commands/roster.cmd";
import { ConfigurationCmd } from "../commands/configuration.cmd";
import { AddAdminCmd } from "../commands/addAdmin.cmd";
import { RemoveAdminCmd } from "../commands/removeAdmin.cmd";

export default (client: Client, db: Database, dbdata: DatabaseData): void => {
  client.on('interactionCreate', async (interaction) => {
    if (!interaction.isCommand()) return;
    const cmdInteraction = interaction as ChatInputCommandInteraction
    const { commandName } = cmdInteraction;
    switch (commandName) {
      case 'newproject':
        await NewProjectCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'addstaff':
        await AddStaffCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'swapstaff':
        await SwapStaffCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'removestaff':
        await RemoveStaffCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'setweight':
        await SetWeightCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'addadditionalstaff':
        await AddAdditionalStaffCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'swapadditionalstaff':
        await SwapAdditionalStaffCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'removeadditionalstaff':
        await RemoveAdditionalStaffCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'addepisode':
        await AddEpisodeCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'removeepisode':
        await RemoveEpisodeCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'addalias':
        await AddAliasCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'removealias':
        await RemoveAliasCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'done':
        await DoneCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'undone':
        await UndoneCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'skip':
        await SkipCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'release':
        await ReleaseCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'transferownership':
        await TransferOwnershipCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'editproject':
        await EditProjectCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'deleteproject':
        await DeleteProjectCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'blame':
        await BlameCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'roster':
        await RosterCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'addobserver':
        await AddObserverCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'removeobserver':
        await RemoveObserverCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'addadmin':
        await AddAdminCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'removeadmin':
        await RemoveAdminCmd(client, db, dbdata, cmdInteraction);
        break;
      case 'help':
        await HelpCmd(cmdInteraction);
        break;
      case 'about':
        await AboutCmd(cmdInteraction);
        break;
      case 'configuration':
        await ConfigurationCmd(client, db, dbdata, cmdInteraction);
        break;
    }
  });

  client.on('interactionCreate', async (interaction: BaseInteraction) => {
    if (!interaction.isAutocomplete()) return;
    const { options, guildId, commandName } = interaction as AutocompleteInteraction;
    let focusedOption = options.getFocused(true);
    let choices;
    try {
      switch (focusedOption.name) {
        case 'project': {
          if (guildId === null || (!(guildId in dbdata.guilds) && !(guildId in dbdata.observers))) break;
          let aliases: string[] = [];

          // own guild
          if ((guildId in dbdata.guilds)) {
            let projects = dbdata.guilds[guildId];
            const newAliases = Object.values(projects).reduce((acc, cur) => {
              acc.push(cur.nickname);
              if (cur.aliases) acc.push(...cur.aliases);
              return acc;
            }, [] as string[]);
            aliases = [...aliases, ...newAliases];
          }

          // Observing guilds
          if (commandName === 'blame' && guildId in dbdata.observers) {
            const guilds = Object.keys(dbdata.observers[guildId]);
            for (let curGuildId of guilds) {
              if (!(curGuildId in dbdata.guilds)) continue;

              let observedProjects = (dbdata.observers[guildId][curGuildId]).filter((p) => p.blame).map(p => p.name);
              let projects = Object.values(dbdata.guilds[curGuildId]).filter(p => observedProjects.includes(p.nickname));

              const newAliases = Object.values(projects).reduce((acc, cur) => {
                acc.push(cur.nickname);
                if (cur.aliases) acc.push(...cur.aliases);
                return acc;
              }, [] as string[]);
              aliases = [...aliases, ...newAliases];
            }
          }
          
          choices = aliases.filter(choice => choice.startsWith(focusedOption.value));
          await interaction.respond(choices.map(choice => ({ name: choice, value: choice })).slice(0, 25));
          return;
        }
        case 'episode': {
          let projectName = await GetAlias(db, dbdata, interaction, options.getString('project')!);
          if (guildId === null || projectName === null || projectName === '') break;
          if (!projectName || !(projectName in dbdata.guilds[guildId])) return;
          let project = dbdata.guilds[guildId][projectName];
          choices = [];
          for (let ep in project.episodes) {
            let num = project.episodes[ep].number;
            if (String(num).startsWith(String(focusedOption.value)))
              choices.push({ name: `${num}`, value: num });
          }
          await interaction.respond(choices.slice(0, 25));
          return;
        }
        case 'abbreviation': {
          let projectName = await GetAlias(db, dbdata, interaction, options.getString('project')!);
          let episode = options.getNumber('episode');
          if (guildId === null || projectName === null || projectName === '') break;
          if (!projectName || !(projectName in dbdata.guilds[guildId])) break;
          let project = dbdata.guilds[guildId][projectName];
          choices = [];

          if (!episode && commandName !== 'done') { // No Additional Staff
            for (let staffId in project.keyStaff) {
              let role = project.keyStaff[staffId].role;
              if (role.abbreviation.startsWith(focusedOption.value.toUpperCase()))
                choices.push({ name: role.abbreviation, value: role.abbreviation });
            }            
          } else {
            for (let ep in project.episodes) {
              let epObj = project.episodes[ep];
              if ((episode != null && epObj.number === episode) || (episode == null && epObj.done == false)) {  // Specified, or first undone
                for (let taskId in project.episodes[ep].tasks) {
                  let task = project.episodes[ep].tasks[taskId];
                  if (task.abbreviation.startsWith(focusedOption.value.toUpperCase()))
                    choices.push({ name: task.abbreviation, value: task.abbreviation });
                }
                break;
              }
            }
          }
          await interaction.respond(choices);
          return;
        }
      }
    } catch (e) {
      await interaction.respond([]);
      return;
    }
    await interaction.respond([]);
    return;
  });
};
