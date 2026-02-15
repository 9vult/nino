// SPDX-License-Identifier: MPL-2.0

using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Nino.Host;

internal sealed class LogProvider
{
    private const string PrintLayout =
        "${longdate} | ${level:uppercase=true:padding=-5} | ${logger}.${callsite:className=false:methodName=true} → ${message}";

    private static readonly JsonLayout FileLayout = new()
    {
        RenderEmptyObject = false,
        Attributes =
        {
            new JsonAttribute("timestamp", "${longdate}"),
            new JsonAttribute("level", "${level:uppercase=true}"),
            new JsonAttribute("origin", "${logger}.${callsite:className=false:methodName=true}"),
            new JsonAttribute("message", "${message}"),
            new JsonAttribute("exception", "${exception:format=toString}"),
            new JsonAttribute(
                "properties",
                new JsonLayout { IncludeEventProperties = true },
                encode: false
            ),
        },
    };

    internal static void Setup()
    {
        var config = new LoggingConfiguration();
        var consoleTarget = new ColoredConsoleTarget("console") { Layout = PrintLayout };
        var fileTarget = new FileTarget("file")
        {
            Layout = FileLayout,
            FileName = Path.Combine("logs", $"{DateTime.Now:yyyy-MM-dd}.jsonl"),
        };

        config.LoggingRules.Add(
            new LoggingRule("Microsoft.*", LogLevel.Info, LogLevel.Debug, new NullTarget())
        );

        config.AddTarget("console", consoleTarget);
        config.AddTarget("file", fileTarget);

        config.AddRuleForAllLevels(consoleTarget);
        config.AddRuleForAllLevels(fileTarget);

        LogManager.Configuration = config;
    }

    private LogProvider() { }
}
