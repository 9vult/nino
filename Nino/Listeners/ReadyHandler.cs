using Discord.Net;
using Newtonsoft.Json;
using Nino.Commands;

namespace Nino.Listeners
{
    internal static partial class Listener
    {
        public static async Task Ready()
        {
            try
            {
                await Nino.Client.CreateGlobalApplicationCommandAsync(ProjectManagement.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(KeyStaff.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(AdditionalStaff.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(ServerManagement.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(Episodes.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(Observer.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(About.Builder.Build());
                await Nino.Client.CreateGlobalApplicationCommandAsync(Help.Builder.Build());
            }
            catch (HttpException e)
            {
                var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
                log.Error(json);
            }
        }
    }
}
