using System.Text;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Blameall(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("blameall", "Check the overall status of a project")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("filter", "Filter results")] BlameAllFilter filter = BlameAllFilter.All,
            [Summary("type", "Display type")] BlameAllType type = BlameAllType.Normal
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = interaction.GuildLocale ?? "en-US";
            alias = alias.Trim();
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction, includeObservers: true);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var episodes = filter switch
            {
                BlameAllFilter.All => (await Getters.GetEpisodes(project)).OrderBy(e => e.Number),
                BlameAllFilter.InProgress => (await Getters.GetEpisodes(project)).Where(e => !e.Done && e.Tasks.Any(t => t.Done)).OrderBy(e => e.Number),
                BlameAllFilter.Incomplete => (await Getters.GetEpisodes(project)).Where(e => !e.Done).OrderBy(e => e.Number),
                _ => (await Getters.GetEpisodes(project)).OrderBy(e => e.Number) // All
            };

            // Calculate pages
            // Thanks to petzku and astiob for their contributions to this algorithm
            var pageCount = Math.Ceiling(episodes.Count() / 13d);
            var pageLength = Math.Floor(episodes.Count() / pageCount);
            var roundUp = episodes.Count() % pageCount;

            // Build the paginator
            var paginator = new LazyPaginatorBuilder()
                .AddUser(Context.User)
                .WithPageFactory(async (int page0) =>
                {
                    var page1 = page0 + 1;
                    var skip = (int)(page0 * pageLength + Math.Min(page0, roundUp));
                    var length = (int)(pageLength + Math.Min(page1, roundUp));

                    var pagedEpisodes = episodes.Skip(skip).Take(length);
                    var progress = await BuildProgress(pagedEpisodes, project, lng, type);

                    // Add the project's MOTD or archival notice, if applicable
                    if (!string.IsNullOrEmpty(project.Motd))
                        progress = $"{project.Motd}\n{progress}";
                    if (project.IsArchived)
                        progress = $"{T("blame.archived", lng)}\n{progress}";

                    return new PageBuilder()
                        .WithAuthor(title)
                        .WithTitle(T("title.blameall", lng))
                        .WithThumbnailUrl(project.PosterUri)
                        .WithDescription(progress)
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
                        .WithThumbnailUrl(project.PosterUri)
                        .WithDescription(T("error.paginatorNoAccess", gLng, userMention))
                        .WithCurrentTimestamp()
                        .Build();
                })
                .Build();

            await _interactiveService.SendPaginatorAsync(paginator, interaction, TimeSpan.FromMinutes(1), InteractionResponseType.DeferredChannelMessageWithSource);

            return ExecutionResult.Success;
        }

        private static async Task<string> BuildProgress(IEnumerable<Episode> pagedEpisodes, Project project, string lng, BlameAllType type)
        {
            StringBuilder sb = new ();
            foreach (var episode in pagedEpisodes)
            {
                sb.Append($"{episode.Number}: ");

                try
                {
                    if (episode.Done)
                        sb.AppendLine($"_{T("blameall.done", lng)}_");
                    else if (episode.Tasks.Any(t => t.Done))
                    {
                        if (type == BlameAllType.Normal)
                            sb.AppendLine(StaffList.GenerateProgress(project, episode));
                        if (type == BlameAllType.StallCheck)
                            sb.AppendLine(T("episode.lastUpdated", lng, $"<t:{episode.Updated?.ToUnixTimeSeconds()}:R>"));
                    }
                    else if (project.AniListId is not null && !await AirDateService.EpisodeAired((int)project.AniListId, episode.Number))
                    {
                        if (type == BlameAllType.Normal)
                            sb.AppendLine($"_{T("blameall.notYetAired", lng)}_");
                        if (type == BlameAllType.StallCheck)
                            sb.AppendLine(await AirDateService.GetAirDateString((int)project.AniListId, episode.Number, lng));
                    }
                    else
                        sb.AppendLine($"_{T("blameall.notStarted", lng)}_");
                }
                catch (Exception e)
                {
                    if (e.Message.StartsWith("error."))
                        sb.AppendLine(T(e.Message, lng));
                }

            }
            return sb.ToString();
        }
    }
}
