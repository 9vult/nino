using Discord;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Nino.Listeners
{
    internal static partial class Listener
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static Task Log(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Info:
                    log.Info(msg.ToString());
                    break;
                case LogSeverity.Warning:
                    log.Warn(msg.ToString());
                    break;
                case LogSeverity.Debug:
                case LogSeverity.Verbose:
                    log.Debug(msg.ToString());
                    break;
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    log.Error(msg.ToString());
                    break;
                default:
                    log.Info(msg.ToString());
                    break;
            }
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
