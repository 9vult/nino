using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Localizer;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands;

public class AtMe(InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("atme", "What tasks are At Me?")]
    public async Task<RuntimeResult> Handle(
        [Summary("type", "Calculation type")] AtMeType type = AtMeType.Auto,
        [Summary("filter", "Filter results")] string? filter = null,
        [Summary("global", "Combine results from all servers")] bool global = false,
        [Summary("private", "Include results from Private projects")] bool? displayPrivateInput = null
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;
        var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";
            
        var displayPrivate = displayPrivateInput ?? !global;
            
        Log.Trace($"Generating At Me for M[{interaction.User.Id} (@{interaction.User.Username})] {{ Type={type}, Global={global}, Private={displayPrivate}, Filter={filter ?? "<none>"} }}");

        var projects = global ? Cache.GetProjects() : Cache.GetProjects(interaction.GuildId ?? 0);
        var episodeCandidates = new Dictionary<Project, List<Episode>>();
        var projectCandidates = (displayPrivate
            ? projects.Where(p => !p.IsArchived)
            : projects.Where(p => p is { IsArchived: false, IsPrivate: false })).ToList();

        // Fuzzy filter project names
        if (filter is not null)
        {
            var matches = new HashSet<string>(FuzzySharp.Process.ExtractSorted(
                filter,
                projectCandidates.SelectMany(p => new[] { p.Title, p.Nickname }),
                cutoff: 70
            ).Select(m => m.Value));
                
            projectCandidates = projectCandidates
                .Where(p => matches.Contains(p.Title) || matches.Contains(p.Nickname))
                .ToList();
        }

        foreach (var project in projectCandidates)
        {
            var episodes = Cache.GetEpisodes(project.Id)
                .Where(e => !e.Done)
                .Where(e => Utils.VerifyEpisodeUser(interaction.User.Id, project, e, true)).ToList();
                
            if (episodes.Count == 0)
                continue;

            if (project.AniListId is not null && project.AniListId > 0)
            {
                try
                {
                    var airedEpisodeNumbers = await AirDateService.AiredEpisodes((int)project.AniListId);
                    if (airedEpisodeNumbers is not null)
                    {
                        episodes = episodes
                            .Where(e => !Utils.EpisodeNumberIsNumber(e.Number, out var dNum)
                                        || airedEpisodeNumbers.Contains(dNum + (project.AniListOffset ?? 0))
                                        || dNum + (project.AniListOffset ?? 0) < airedEpisodeNumbers.Max()).ToList();
                    }
                }
                catch (Exception e)
                {
                    return await Response.Fail(T(e.Message, lng), interaction);
                }
            }

            episodeCandidates.Add(project, episodes.ToList());
        }

        var usingConga = true;
        var empty = false;
        List<string> results = [];
        switch (type)
        {
            case AtMeType.Auto:
                GetCongaResults();
                if (results.Count > 0) break;
                GetIncompleteResults();
                usingConga = false;
                break;
            case AtMeType.Conga:
                GetCongaResults();
                break;
            case AtMeType.Incomplete:
                GetIncompleteResults();
                usingConga = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
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
                var length = (int)(pageLength + (page1 <= roundUp ? 1 : 0));

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

        await interactive.SendPaginatorAsync(paginator, interaction, TimeSpan.FromMinutes(1), InteractionResponseType.DeferredChannelMessageWithSource);

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

                    var tardyTasks = CongaHelper.GetTardyTasks(project, episode, false)
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