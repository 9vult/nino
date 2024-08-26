using Discord.Net;
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
        public static async Task Ready()
        {
            try
            {
                await Nino.Client.CreateGlobalApplicationCommandAsync(NewProject.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(KeyStaff.Builder.Build());
            }
            catch (HttpException e)
            {
                var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
                log.Error(json);
            }
        }
    }
}
