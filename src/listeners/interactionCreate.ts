import { Client, EmbedBuilder, Message, MessageMentionOptions, GuildMemberRoleManager, User, BaseInteraction, AutocompleteInteraction } from "discord.js";
import type { Database } from '@firebase/database-types';

import { DatabaseData } from "../misc/types";
import { NewProjectCmd } from "../commands/newProject.cmd";
import { AddStaffCmd } from "../commands/addStaff.cmd";
import { AddAdditionalStaffCmd } from "../commands/addAdditionalStaff.cmd";
import { HelpCmd } from "../commands/help.cmd";
import { AboutCmd } from "../commands/about.cmd";

export default (client: Client, db: Database, dbdata: DatabaseData): void => {
  client.on('interactionCreate', async (interaction) => {
    if (!interaction.isCommand()) return;
    const { commandName } = interaction;
    switch (commandName) {
      case 'newproject':
        await NewProjectCmd(client, db, dbdata, interaction);
        break;
      case 'addstaff':
        await AddStaffCmd(client, db, dbdata, interaction);
        break;
      case 'addadditionalstaff':
        await AddAdditionalStaffCmd(client, db, dbdata, interaction);
        break;
      case 'help':
        await HelpCmd(interaction);
        break;
      case 'about':
        await AboutCmd(interaction);
        break;
    }
  });

  client.on('interactionCreate', async (interaction: BaseInteraction) => {
    if (!interaction.isAutocomplete()) return;

    const { options, guildId } = interaction as AutocompleteInteraction;

    let focusedOption = options.getFocused(true);
    switch (focusedOption.name) {
      case 'project':
        if (guildId == null || !(guildId in dbdata.guilds)) return;
        let projects = dbdata.guilds[guildId];
        let choices = Object.keys(projects).filter(choice => choice.startsWith(focusedOption.value));
        await interaction.respond(choices.map(choice => ({ name: choice, value: choice })));
        break;
    }
  });
};
