// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Entities;

public sealed record FailureContext
{
    public Alias? Alias { get; init; }
    public Number? Episode { get; init; }
    public Abbreviation? Task { get; init; }
    public Dictionary<ResultStatus, string>? Overrides { get; init; }
    public Dictionary<string, object> Arguments { get; init; } = [];

    public FailureContext() { }

    public FailureContext(Alias? alias, Number? episode, Abbreviation? task)
    {
        Alias = alias;
        Episode = episode;
        Task = task;
    }

    internal Dictionary<string, object> ToLocalizationArgs()
    {
        Dictionary<string, object> args = new()
        {
            ["alias"] = Alias?.Value ?? "[Project]",
            ["nickname"] = Alias?.Value ?? "[Project]",
            ["episode"] = Episode?.Value ?? "[Episode]",
            ["abbreviation"] = Task?.Value ?? "[Task]",
        };

        foreach (var (k, v) in Arguments)
            args[k] = v;

        return args;
    }
};
