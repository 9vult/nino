// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;

namespace Nino.Discord.Interactions;

public sealed class ExecutionResult : RuntimeResult
{
    /// <inheritdoc />
    public ExecutionResult(InteractionCommandError? error, string reason)
        : base(error, reason) { }

    public static ExecutionResult Success => new(null, "Success");
    public static ExecutionResult Failure => new(null, "Failure");
}
