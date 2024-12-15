using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Selection;
using Nino.Handlers;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Help (InteractionHandler handler, InteractionService commands, InteractiveService interactiveService)
        : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("help", "Nino Help")]
        public async Task<RuntimeResult> Handle ()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var localizedTitles = 
                Titles.Select(key =>  new SelectionOption(key, T(key, lng), key.Replace(".title", ".body")))
                .ToArray();

            var pageBuilder = new PageBuilder()
                .WithTitle(T("title.help", lng))
                .WithDescription(T("nino.help.body", lng));

            var selection = new SelectionBuilder<SelectionOption>()
                .AddUser(Context.User)
                .WithOptions(localizedTitles)
                .WithStringConverter(x => x.Title)
                .WithInputType(InputType.SelectMenus)
                .WithActionOnSuccess(ActionOnStop.DeleteInput)
                .WithPlaceholder(T("nino.help.prompt", lng))
                .WithSelectionPage(pageBuilder)
                .WithMaxValues(1)
                .Build();

            var result = await interactiveService.SendSelectionAsync(selection, interaction, TimeSpan.FromMinutes(1),
                InteractionResponseType.DeferredChannelMessageWithSource);

            if (result?.IsSuccess ?? false)
            {
                var value = result!.Value;
                var embed = new EmbedBuilder()
                    .WithTitle(T("nino.help.prompt.filled", lng, value.Title))
                    .WithDescription(T(value.BodyKey, lng))
                    .Build();
                await interaction.ModifyOriginalResponseAsync(m => { m.Embed = embed; });
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.help", lng))
                    .WithDescription(T("progress.done.inTheDust.timeout", lng))
                    .Build();
                await interaction.ModifyOriginalResponseAsync(m => { m.Embed = embed; });
            }

            return ExecutionResult.Success;
        }
        
        // @formatter:off
        private static readonly string[] Titles =
        [
            "nino.help.blame.title",
            "nino.help.done.title",
            "nino.help.release.title",
            "nino.help.observer.title",
            "nino.help.create.title",
            "nino.help.conga.title",
            "nino.help.weight.title",
            "nino.help.airtime.title",
            "nino.help.airreminder.title",
            "nino.help.staffswap.title",
            "nino.help.roster.title",
            "nino.help.admin.title",
            "nino.help.customize.title",
            "nino.help.help.title",
            "nino.help.language.title",
            "nino.help.localize.title",
            "nino.help.transfer.title"
        ];
        // @formatter:on

        private struct SelectionOption (string key, string title, string bodyKey)
        {
            public readonly string Key = key;
            public readonly string Title = title;
            public readonly string BodyKey = bodyKey;
        }
    }
}
