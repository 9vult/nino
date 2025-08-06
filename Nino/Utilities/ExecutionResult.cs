using Discord.Interactions;

namespace Nino.Utilities
{
    public class ExecutionResult : RuntimeResult
    {
        public ExecutionResult(InteractionCommandError? error, string reason) : base(error, reason)
        {
        }

        public static ExecutionResult Success => new(null, "Success");

        public static ExecutionResult Failure => new(InteractionCommandError.Unsuccessful, "Failure");
    }
}
