// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Serilog.Events;
using Serilog.Formatting;

namespace Nino.Host.Logging;

public sealed class NinoJsonFormatter : ITextFormatter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    private static readonly HashSet<string> CoreProperties =
    [
        "SourceContext",
        "ActionId",
        "ActionName",
        "RequestId",
        "RequestPath",
        "ConnectionId",
        "MachineName",
        "ThreadId",
    ];

    /// <inheritdoc />
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var core = new Dictionary<string, object?>
        {
            ["@t"] = logEvent.Timestamp.UtcDateTime.ToString("O"),
            ["@mt"] = logEvent.MessageTemplate.Text,
            // ["@m"] = logEvent.RenderMessage(),
            ["@l"] = logEvent.Level.ToString(),
        };

        if (logEvent.Exception is not null)
            core["@x"] = logEvent.Exception.ToString();

        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
            core["SourceContext"] = RenderValue(sourceContext);

        // Everything else goes into context
        var context = new Dictionary<string, object?>();
        foreach (var (key, value) in logEvent.Properties)
        {
            if (CoreProperties.Contains(key))
                continue;

            context[key] = RenderValue(value);
        }

        if (context.Count > 0)
            core["context"] = context;

        output.WriteLine(JsonSerializer.Serialize(core));
    }

    private static object? RenderValue(LogEventPropertyValue value) =>
        value switch
        {
            ScalarValue scalar => scalar.Value,
            SequenceValue seq => seq.Elements.Select(RenderValue).ToList(),
            StructureValue str => str.Properties.ToDictionary(
                p => p.Name,
                p => RenderValue(p.Value)
            ),
            DictionaryValue d => d.Elements.ToDictionary(
                kv => kv.Key.Value?.ToString() ?? "",
                kv => RenderValue(kv.Value)
            ),
            _ => value.ToString(),
        };
}
