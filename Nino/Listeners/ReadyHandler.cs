using Discord.Net;
using Newtonsoft.Json;
using Nino.Commands;

namespace Nino.Listeners
{
    internal static partial class Listener
    {
        public static async Task Ready()
        {
            // (Re)deploy slash commands if deploy-commands flag is set
            if (Nino.CmdLineOptions.DeployCommands)
            {
                try
                {
                    log.Info("--deploy-commands is set. Deploying slash commands...");
                    await Nino.Client.CreateGlobalApplicationCommandAsync(ProjectManagement.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(KeyStaff.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(AdditionalStaff.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(ServerManagement.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(Episodes.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(Observer.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(Release.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(Roster.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(About.Builder.Build());
                    await Nino.Client.CreateGlobalApplicationCommandAsync(Help.Builder.Build());
                    log.Info("Slash commands deployed");
                }
                catch (HttpException e)
                {
                    var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
                    log.Error(json);
                }
            }
        }
    }
}
