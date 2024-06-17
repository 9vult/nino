import { Client, SlashCommandBuilder } from "discord.js";
import { GetDescriptions as GetDescs, GetNames, LoadCmdI18Ns } from "../actions/i18n.action";
import { DatabaseData } from "../misc/types";

export default (client: Client, dbdata: DatabaseData): void => {
  client.on('ready', async () => {
    if (!client.user || !client.application) return;

    const d = dbdata.i18n;

    const helpCmd = new SlashCommandBuilder()
      .setName('help')
      .setDescription('Nino Help')
      .setNameLocalizations(GetNames(d, 'commands', 'help'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'help'));

    const aboutCmd = new SlashCommandBuilder()
      .setName('about')
      .setDescription('About Nino')
      .setNameLocalizations(GetNames(d, 'commands', 'about'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'about'));

    const newProjectCmd = new SlashCommandBuilder()
      .setName('newproject')
      .setDescription('Create a new project')
      .setNameLocalizations(GetNames(d, 'commands', 'newproject'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'newproject'))
      .addStringOption(o =>
        o.setName('nickname')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'nickname'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'nickname'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('title')
          .setDescription('Full series title')
          .setNameLocalizations(GetNames(d, 'options', 'title'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'title'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('projecttype')
          .setDescription('Project type')
          .setNameLocalizations(GetNames(d, 'options', 'projecttype'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'projecttype'))
          .setRequired(true)
          .setChoices(
            { name: 'TV', value: 'TV' },
            { name: 'Movie', value: 'Movie' },
            { name: 'BD', value: 'BD' }
          )
      )
      .addNumberOption(o =>
        o.setName('length')
          .setDescription('Number of episodes')
          .setNameLocalizations(GetNames(d, 'options', 'length'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'length'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('poster')
          .setDescription('Poster image URL')
          .setNameLocalizations(GetNames(d, 'options', 'poster'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'poster'))
          .setRequired(true)
      )
      .addChannelOption(o =>
        o.setName('updatechannel')
          .setDescription('Channel to post updates to')
          .setNameLocalizations(GetNames(d, 'options', 'updatechannel'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'updatechannel'))
          .setRequired(true)
      )
      .addChannelOption(o =>
        o.setName('releasechannel')
          .setDescription('Channel to post releases to')
          .setNameLocalizations(GetNames(d, 'options', 'releasechannel'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'releasechannel'))
          .setRequired(true)
      );

    const addStaffCmd = new SlashCommandBuilder()
      .setName('addstaff')
      .setDescription('Add staff to a project')
      .setNameLocalizations(GetNames(d, 'commands', 'addstaff'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'addstaff'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('name')
          .setDescription('Full position name')
          .setNameLocalizations(GetNames(d, 'options', 'name'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'name'))
          .setRequired(true)
      );

    const removeStaffCmd = new SlashCommandBuilder()
      .setName('removestaff')
      .setDescription('Remove staff from a project')
      .setNameLocalizations(GetNames(d, 'commands', 'removestaff'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'removestaff'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
      );

    const setWeightCmd = new SlashCommandBuilder()
      .setName('setweight')
      .setDescription('Set the weight of a Key Staff position')
      .setNameLocalizations(GetNames(d, 'commands', 'setweight'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'setweight'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('weight')
          .setDescription('Weight')
          .setNameLocalizations(GetNames(d, 'options', 'weight'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'weight'))
          .setRequired(true)
      );

    const swapStaffCmd = new SlashCommandBuilder()
      .setName('swapstaff')
      .setDescription('Swap staff into a project')
      .setNameLocalizations(GetNames(d, 'commands', 'swapstaff'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'swapstaff'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      );

    const addAdditionalStaffCmd = new SlashCommandBuilder()
      .setName('addadditionalstaff')
      .setDescription('Add additional staff to an episode')
      .setNameLocalizations(GetNames(d, 'commands', 'addadditionalstaff'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'addadditionalstaff'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('name')
          .setDescription('Full position name')
          .setNameLocalizations(GetNames(d, 'options', 'name'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'name'))
          .setRequired(true)
      );

    const removeAdditionalStaffCmd = new SlashCommandBuilder()
      .setName('removeadditionalstaff')
      .setDescription('Remove additional staff from an episode')
      .setNameLocalizations(GetNames(d, 'commands', 'removeadditionalstaff'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'removeadditionalstaff'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
      );

    const swapAdditionalStaffCmd = new SlashCommandBuilder()
      .setName('swapadditionalstaff')
      .setDescription('Swap additional staff into an episode')
      .setNameLocalizations(GetNames(d, 'commands', 'swapadditionalstaff'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'swapadditionalstaff'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      );

    const transferOwnershipCmd = new SlashCommandBuilder()
      .setName('transferownership')
      .setDescription('Transfer project ownership to someone else')
      .setNameLocalizations(GetNames(d, 'commands', 'transferownership'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'transferownership'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      );

    const deleteProjectCmd = new SlashCommandBuilder()
      .setName('deleteproject')
      .setDescription('Delete a project')
      .setNameLocalizations(GetNames(d, 'commands', 'deleteproject'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'deleteproject'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      );

    const editProjectCmd = new SlashCommandBuilder()
      .setName('editproject')
      .setDescription('Edit a project')
      .setNameLocalizations(GetNames(d, 'commands', 'editproject'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'editproject'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('option')
          .setDescription('Option to change')
          .setRequired(true)
          .addChoices(
            { name: 'Title', value: 'Title' },
            { name: 'Poster', value: 'Poster' },
            { name: 'AniDB', value: 'AniDB' },
            { name: 'AirTime24h', value: 'AirTime24h' },
            { name: 'UpdateChannelID', value: 'UpdateChannel' },
            { name: 'ReleaseChannelID', value: 'ReleaseChannel' }
          )
      )
      .addStringOption(o =>
        o.setName('newvalue')
          .setDescription('New value')
          .setNameLocalizations(GetNames(d, 'options', 'newvalue'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'newvalue'))
          .setRequired(true)
      );

    const addEpisodeCmd = new SlashCommandBuilder()
      .setName('addepisode')
      .setDescription('Add an episode')
      .setNameLocalizations(GetNames(d, 'commands', 'addepisode'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'addepisode'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
      );

    const removeEpisodeCmd = new SlashCommandBuilder()
      .setName('removeepisode')
      .setDescription('Remove an episode')
      .setNameLocalizations(GetNames(d, 'commands', 'removeepisode'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'removeepisode'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
      );

    const addAliasCmd = new SlashCommandBuilder()
      .setName('addalias')
      .setDescription('Add an alias')
      .setNameLocalizations(GetNames(d, 'commands', 'addalias'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'addalias'))
      .addStringOption(o => 
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('alias')
          .setDescription('Alias')
          .setNameLocalizations(GetNames(d, 'options', 'alias'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'alias'))
          .setRequired(true)
      );

    const removeAliasCmd = new SlashCommandBuilder()
      .setName('removealias')
      .setDescription('Remove an alias')
      .setNameLocalizations(GetNames(d, 'commands', 'removealias'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'removealias'))
      .addStringOption(o => 
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('alias')
          .setDescription('Alias')
          .setNameLocalizations(GetNames(d, 'options', 'alias'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'alias'))
          .setRequired(true)
      );

    const doneCmd = new SlashCommandBuilder()
      .setName('done')
      .setDescription('Mark a position as done')
      .setNameLocalizations(GetNames(d, 'commands', 'done'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'done'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(false)
          .setAutocomplete(true)
      );

    const undoneCmd = new SlashCommandBuilder()
      .setName('undone')
      .setDescription('Mark a position as not done')
      .setNameLocalizations(GetNames(d, 'commands', 'undone'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'undone'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
          .setAutocomplete(true)
      );

    const skipCmd = new SlashCommandBuilder()
      .setName('skip')
      .setDescription('Skip a position')
      .setNameLocalizations(GetNames(d, 'commands', 'skip'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'skip'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setNameLocalizations(GetNames(d, 'options', 'abbreviation'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'abbreviation'))
          .setRequired(true)
          .setAutocomplete(true)
      );

    const blameCmd = new SlashCommandBuilder()
      .setName('blame')
      .setDescription('Check the status of a project')
      .setNameLocalizations(GetNames(d, 'commands', 'blame'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'blame'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project name')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(false)
      )
      .addBooleanOption(o =>
        o.setName('explain')
          .setDescription('Explain what any of this even means')
          .setNameLocalizations(GetNames(d, 'options', 'explain'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'explain'))
          .setRequired(false)
      );

    const rosterCmd = new SlashCommandBuilder()
      .setName('roster')
      .setDescription('See who\'s working on an episode')
      .setNameLocalizations(GetNames(d, 'commands', 'roster'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'roster'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project name')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setNameLocalizations(GetNames(d, 'options', 'episode'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'episode'))
          .setRequired(true)
      )

    const addObserverCmd = new SlashCommandBuilder()
      .setName('addobserver')
      .setDescription('Add a server to observe this project')
      .setNameLocalizations(GetNames(d, 'commands', 'addobserver'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'addobserver'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project name')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('guild')
          .setDescription('Guild ID')
          .setNameLocalizations(GetNames(d, 'options', 'guild'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'guild'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('updates')
          .setDescription('Webhook URL for updates')
          .setNameLocalizations(GetNames(d, 'options', 'updates'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'updates'))
          .setRequired(false)
      )
      .addStringOption(o =>
        o.setName('releases')
          .setDescription('Webhook URL for releases')
          .setNameLocalizations(GetNames(d, 'options', 'releases'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'releases'))
          .setRequired(false)
      );

    const removeObserverCmd = new SlashCommandBuilder()
      .setName('removeobserver')
      .setDescription('Remove a server observing this project')
      .setNameLocalizations(GetNames(d, 'commands', 'removeobserver'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'removeobserver'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project name')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('guild')
          .setDescription('Guild ID')
          .setNameLocalizations(GetNames(d, 'options', 'guild'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'guild'))
          .setRequired(true)
      );

    const addAdminCmd = new SlashCommandBuilder()
      .setName('addadmin')
      .setDescription('Add an administrator to this project')
      .setNameLocalizations(GetNames(d, 'commands', 'addadmin'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'addadmin'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project name')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      );

    const removeAdminCmd = new SlashCommandBuilder()
      .setName('removeadmin')
      .setDescription('Remove an administrator from this project')
      .setNameLocalizations(GetNames(d, 'commands', 'removeadmin'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'removeadmin'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project name')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setNameLocalizations(GetNames(d, 'options', 'member'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'member'))
          .setRequired(true)
      );

    const releaseCmd = new SlashCommandBuilder()
      .setName('release')
      .setDescription('Release!')
      .setNameLocalizations(GetNames(d, 'commands', 'release'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'release'))
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setNameLocalizations(GetNames(d, 'options', 'project'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'project'))
          .setRequired(true)
          .setAutocomplete(true)
      )
      .addStringOption(o =>
        o.setName('type')
          .setDescription('Type of release')
          .setNameLocalizations(GetNames(d, 'options', 'type'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'type'))
          .setRequired(true)
          .addChoices(
            { name: 'Episode', value: 'Episode' },
            { name: 'Volume', value: 'Volume' },
            { name: 'Batch', value: 'Batch' }
          )
      )
      .addStringOption(o =>
        o.setName('number')
          .setDescription('What is being released? [Number or Range]')
          .setNameLocalizations(GetNames(d, 'options', 'number'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'number'))
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('url')
          .setDescription('Release URL')
          .setNameLocalizations(GetNames(d, 'options', 'url'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'url'))
          .setRequired(true)
      )
      .addRoleOption(o =>
        o.setName('role')
          .setDescription('Role to ping')
          .setNameLocalizations(GetNames(d, 'options', 'role'))
          .setDescriptionLocalizations(GetDescs(d, 'options', 'role'))
          .setRequired(false)
      );

    const configurationCmd = new SlashCommandBuilder()
      .setName('configuration')
      .setDescription('Guild-level configuration')
      .setNameLocalizations(GetNames(d, 'commands', 'configuration'))
      .setDescriptionLocalizations(GetDescs(d, 'commands', 'configuration'))
      .addSubcommand(s => 
        s.setName('progress_display')
        .setDescription('Select a progress embed type')
        .setNameLocalizations(GetNames(d, 'commands', 'progress_display'))
        .setDescriptionLocalizations(GetDescs(d, 'commands', 'progress_display'))
        .addStringOption(o =>
          o.setName('embed_type')
            .setDescription('Embed type')
            .setNameLocalizations(GetNames(d, 'options', 'embed_type'))
            .setDescriptionLocalizations(GetDescs(d, 'options', 'embed_type'))
            .setRequired(true)
            .addChoices(
              { name: 'Normal', value: 'Normal' },
              { name: 'Extended', value: 'Extended' }
            )
        )
      );

    client.application.commands.create(helpCmd);
    client.application.commands.create(aboutCmd);
    client.application.commands.create(newProjectCmd);
    client.application.commands.create(addStaffCmd);
    client.application.commands.create(removeStaffCmd);
    client.application.commands.create(setWeightCmd);
    client.application.commands.create(swapStaffCmd);
    client.application.commands.create(addAdditionalStaffCmd);
    client.application.commands.create(removeAdditionalStaffCmd);
    client.application.commands.create(swapAdditionalStaffCmd);
    client.application.commands.create(transferOwnershipCmd);
    client.application.commands.create(deleteProjectCmd);
    client.application.commands.create(editProjectCmd);
    client.application.commands.create(addEpisodeCmd);
    client.application.commands.create(removeEpisodeCmd);
    client.application.commands.create(addAliasCmd);
    client.application.commands.create(removeAliasCmd);
    client.application.commands.create(doneCmd);
    client.application.commands.create(undoneCmd);
    client.application.commands.create(skipCmd);
    client.application.commands.create(blameCmd);
    client.application.commands.create(rosterCmd);
    client.application.commands.create(releaseCmd);
    client.application.commands.create(addObserverCmd);
    client.application.commands.create(removeObserverCmd);
    client.application.commands.create(addAdminCmd);
    client.application.commands.create(removeAdminCmd);
    client.application.commands.create(configurationCmd);

    console.log('Nino is ready to go!');
  });
};

