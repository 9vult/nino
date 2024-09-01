using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Help(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        [SlashCommand("help", "Nino Help")]
        public async Task<RuntimeResult> Handle()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            var embed = new EmbedBuilder()
                .WithTitle(T("title.help", lng))
                .WithDescription(T("nino.help", lng))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
