using Discord;
using NLog.Config;
using NLog.Targets;
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
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static Task Log(LogMessage msg)
        {
            log.Debug(msg.ToString());
            return Task.CompletedTask;
        }

        public static void SetupLogger()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("console") { Layout = "${longdate} [${level}] ${message}" };

            config.AddTarget(consoleTarget);
            config.AddRuleForAllLevels(consoleTarget);
            LogManager.Configuration = config;
        }
    }
}
