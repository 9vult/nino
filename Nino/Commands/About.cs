using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class About(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        [SlashCommand("about", "About Nino")]
        public async Task<RuntimeResult> Handle()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            var embed = new EmbedBuilder()
                .WithTitle(T("title.about", lng))
                .WithDescription(T("nino.about", lng, Utils.VERSION))
                .WithUrl("https://github.com/9vult/nino")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
