using Discord.Net;
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
                case ProjectManagement.Name:
                    await ProjectManagement.Handle(interaction);
                    break;
                case KeyStaff.Name:
                    await KeyStaff.Handle(interaction);
                    break;
            }
        }
    }
}
