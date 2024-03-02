import { Client, EmbedBuilder, Message, MessageMentionOptions, GuildMemberRoleManager, User } from "discord.js";
import type { Database } from '@firebase/database-types';

import { DatabaseData } from "../misc/types";
import { NewProjectCmd } from "src/commands/newProject.cmd";
import { AddStaffCmd } from "src/commands/addStaff.cmd";
import { AddAdditionalStaffCmd } from "src/commands/addAdditionalStaff.cmd";
import { HelpCmd } from "src/commands/help.cmd";
import { AboutCmd } from "src/commands/about.cmd";

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
};
