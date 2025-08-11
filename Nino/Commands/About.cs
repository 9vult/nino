using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class About : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [SlashCommand("about", "About Nino")]
        public async Task<RuntimeResult> Handle()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            Log.Trace(
                $"Displaying About to M[{interaction.User.Id} (@{interaction.User.Username})]"
            );

            var embed = new EmbedBuilder()
                .WithTitle(T("title.about", lng))
                .WithDescription(T("nino.about", lng, Utils.Version))
                .WithUrl("https://github.com/9vult/nino")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
