import { Client, SlashCommandBuilder } from "discord.js";

export default (client: Client): void => {
  client.on('ready', async () => {
    if (!client.user || !client.application) return;

    const helpCmd = new SlashCommandBuilder()
      .setName('help')
      .setDescription('Nino Help');

    const aboutCmd = new SlashCommandBuilder()
      .setName('about')
      .setDescription('About Nino');

    const newProjectCmd = new SlashCommandBuilder()
      .setName('newproject')
      .setDescription('Create a new project')
      .addStringOption(o =>
        o.setName('nickname')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('title')
          .setDescription('Full series title')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('type')
          .setDescription('Project type')
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
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('poster')
          .setDescription('Poster image URL')
          .setRequired(true)
      )
      .addChannelOption(o =>
        o.setName('updatechannel')
          .setDescription('Channel to post updates to')
          .setRequired(true)
      )
      .addChannelOption(o =>
        o.setName('releasechannel')
          .setDescription('Channel to post releases to')
          .setRequired(true)
      );

    const addStaffCmd = new SlashCommandBuilder()
      .setName('addstaff')
      .setDescription('Add staff to a project')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('title')
          .setDescription('Full position name')
          .setRequired(true)
      );

    const removeStaffCmd = new SlashCommandBuilder()
      .setName('removestaff')
      .setDescription('Remove staff from a project')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      );

    const swapStaffCmd = new SlashCommandBuilder()
      .setName('swapstaff')
      .setDescription('Swap staff into a project')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setRequired(true)
      );

    const addAdditionalStaffCmd = new SlashCommandBuilder()
      .setName('addadditionalstaff')
      .setDescription('Add additional staff to an episode')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('title')
          .setDescription('Full position name')
          .setRequired(true)
      );

    const removeAdditionalStaffCmd = new SlashCommandBuilder()
      .setName('removeadditionalstaff')
      .setDescription('Remove additional staff from an episode')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      );

    const swapAdditionalStaffCmd = new SlashCommandBuilder()
      .setName('swapadditionalstaff')
      .setDescription('Swap additional staff into an episode')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setRequired(true)
      );

    const transferOwnershipCmd = new SlashCommandBuilder()
      .setName('transferownership')
      .setDescription('Transfer project ownership to someone else')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addUserOption(o =>
        o.setName('member')
          .setDescription('Staff member')
          .setRequired(true)
      );

    const deleteProjectCmd = new SlashCommandBuilder()
      .setName('deleteproject')
      .setDescription('Delete a project')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      );

    const editProjectCmd = new SlashCommandBuilder()
      .setName('editproject')
      .setDescription('Edit a project')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('option')
          .setDescription('Option to change')
          .setRequired(true)
          .addChoices(
            { name: 'Length', value: 'Length' },
            { name: 'Poster', value: 'Poster' },
            { name: 'UpdateChannel', value: 'UpdateChannel' },
            { name: 'ReleaseChannel', value: 'ReleaseChannel' }
          )
      )
      .addStringOption(o =>
        o.setName('newvalue')
          .setDescription('New value')
          .setRequired(true)
      );

    const doneCmd = new SlashCommandBuilder()
      .setName('done')
      .setDescription('Mark a position as done')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      );

    const undoneCmd = new SlashCommandBuilder()
      .setName('undone')
      .setDescription('Mark a position as not done')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addNumberOption(o =>
        o.setName('episode')
          .setDescription('Episode number')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('abbreviation')
          .setDescription('Position shorthand')
          .setRequired(true)
      );

    const releaseCmd = new SlashCommandBuilder()
      .setName('release')
      .setDescription('Release!')
      .addStringOption(o =>
        o.setName('project')
          .setDescription('Project nickname')
          .setRequired(true)
      )
      .addStringOption(o =>
        o.setName('type')
          .setDescription('Type of release')
          .setRequired(true)
          .addChoices(
            { name: 'Episode', value: 'Episode' },
            { name: 'Batch', value: 'Batch' }
          )
      )
      .addStringOption(o =>
        o.setName('number')
          .setDescription('What is being released?')
          .setRequired(true)
      );

    client.application.commands.create(helpCmd);
    client.application.commands.create(aboutCmd);
    client.application.commands.create(newProjectCmd);
    client.application.commands.create(addStaffCmd);
    client.application.commands.create(removeStaffCmd);
    client.application.commands.create(swapStaffCmd);
    client.application.commands.create(addAdditionalStaffCmd);
    client.application.commands.create(removeAdditionalStaffCmd);
    client.application.commands.create(swapAdditionalStaffCmd);
    client.application.commands.create(transferOwnershipCmd);
    client.application.commands.create(deleteProjectCmd);
    client.application.commands.create(editProjectCmd);
    client.application.commands.create(doneCmd);
    client.application.commands.create(undoneCmd);
    client.application.commands.create(releaseCmd);

    console.log('Nino is ready to go!');
  });
};

