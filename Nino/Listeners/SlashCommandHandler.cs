﻿using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Nino.Commands;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Listeners
{
    internal static partial class Listener
    {
        public static async Task SlashCommandExecuted(SocketSlashCommand interaction)
        {
            await interaction.DeferAsync();

            switch (interaction.CommandName)
            {
                case NewProject.Name:
                    await NewProject.Handle(interaction);
                    break;
            }
        }
    }
}
