using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using NaturalSort.Extension;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class AtMe(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [SlashCommand("atme", "What tasks are At Me?")]
        public async Task<RuntimeResult> Handle(
            [Summary("filter", "Filter results")] AtMeFilter filter = AtMeFilter.Auto,
            [Summary("global", "Combine results from all servers")] bool global = false,
            [Summary("private", "Include results from Private projects")] bool? displayPrivateInput = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";
            
            Log.Trace($"Generating At Me for M[{interaction.User.Id} (@{interaction.User.Username})]");
            
            var displayPrivate = displayPrivateInput ?? !global;

            var projects = global ? Cache.GetProjects() : Cache.GetProjects(interaction.GuildId ?? 0);
            var episodeCandidates = (displayPrivate
                    ? projects.Where(p => !p.IsArchived)
                    : projects.Where(p => p is { IsArchived: false, IsPrivate: false }))
                .ToDictionary(
                    project => project,
                    project => Cache.GetEpisodes(project.Id)
                        .Where(e => (project.AirReminderEnabled && e.ReminderPosted) || !project.AirReminderEnabled)
                        .Where(e => Utils.VerifyUser(interaction.User.Id, project, true, true))
                        .ToList()
                );

            var usingConga = true;
            var empty = false;
            List<string> results = [];
            switch (filter)
            {
                case AtMeFilter.Auto:
                    GetCongaResults();
                    if (results.Count > 0) break;
                    GetIncompleteResults();
                    usingConga = false;
                    break;
                case AtMeFilter.Conga:
                    GetCongaResults();
                    break;
                case AtMeFilter.Incomplete:
                    GetIncompleteResults();
                    usingConga = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }

            if (results.Count == 0)
            {
                empty = true;
                results.Add(T("atMe.empty", lng));
            }
            

            // Calculate pages
            // Thanks to petzku and astiob for their contributions to this algorithm
            var pageCount = Math.Ceiling(results.Count / 13d);
            var pageLength = Math.Floor(results.Count / pageCount);
            var roundUp = results.Count % pageCount;

            // Build the paginator
            var paginator = new LazyPaginatorBuilder()
                .AddUser(Context.User)
                .WithPageFactory((int page0) =>
                {
                    var page1 = page0 + 1;
                    var skip = (int)(page0 * pageLength + Math.Min(page0, roundUp));
                    var length = (int)(pageLength + Math.Min(page1, roundUp));

                    var pagedTasks = results.Skip(skip).Take(length);
                    var body = string.Join('\n', pagedTasks);
                    
                    return new PageBuilder()
                        .WithTitle(T(empty ? "title.atMe.empty" : usingConga ? "title.atMe.conga" : "title.atMe.incomplete", lng))
                        .WithDescription(body)
                        .WithCurrentTimestamp();
                })
                .WithMaxPageIndex((int)pageCount - 1)
                .AddOption(new Emoji("◀"), PaginatorAction.Backward, ButtonStyle.Secondary)
                .AddOption(new Emoji("▶"), PaginatorAction.Forward, ButtonStyle.Secondary)
                .WithActionOnTimeout(ActionOnStop.DeleteInput)
                .WithRestrictedPageFactory((IReadOnlyCollection<IUser> users) =>
                {
                    var userMention = users.Count > 0 ? $"<@{users.First().Id}>" : "unknown_user";
                    return new PageBuilder()
                        .WithTitle(T("title.paginatorNoAccess", gLng))
                        .WithDescription(T("error.paginatorNoAccess", gLng, userMention))
                        .WithCurrentTimestamp()
                        .Build();
                })
                .Build();

            await _interactiveService.SendPaginatorAsync(paginator, interaction, TimeSpan.FromMinutes(1), InteractionResponseType.DeferredChannelMessageWithSource);

            return ExecutionResult.Success;

            // Get results based on Conga
            void GetCongaResults ()
            {
                foreach (var (project, episodes) in episodeCandidates)
                {
                    foreach (var episode in episodes)
                    {
                        var abbreviations = project.KeyStaff
                            .Concat(episode.AdditionalStaff)
                            .Where(s => s.UserId == interaction.User.Id)
                            .Select(p => p.Role.Abbreviation)
                            .ToList();

                        if (abbreviations.Count == 0) continue;

                        var tardyTasks = Utils.GetTardyTasks(project, episode, false)
                            .Where(t => abbreviations.Contains(t))
                            .Select(t => $"`{t}`")
                            .ToList();
                        
                        if (tardyTasks.Count == 0) continue;
                        results.Add(T("atMe.entry", lng, project.Nickname, episode.Number, string.Join(", ", tardyTasks)));
                    }
                }
            }
            
            // Get results based on incomplete tasks
            void GetIncompleteResults ()
            {
                foreach (var (project, episodes) in episodeCandidates)
                {
                    foreach (var episode in episodes)
                    {
                        var abbreviations = project.KeyStaff
                            .Concat(episode.AdditionalStaff)
                            .Where(s => s.UserId == interaction.User.Id)
                            .Select(p => p.Role.Abbreviation)
                            .ToList();

                        var tardyTasks = episode.Tasks
                            .Where(t => !t.Done && abbreviations.Contains(t.Abbreviation))
                            .Select(t => $"`{t.Abbreviation}`")
                            .ToList();
                        
                        if (tardyTasks.Count == 0) continue;
                        results.Add(T("atMe.entry", lng, project.Nickname, episode.Number, string.Join(", ", tardyTasks)));
                    }
                }
            }
        }
    }
}
