using System.Text;
using System.Xml.Linq;
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
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            alias = alias.Trim();
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction, includeObservers: true);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var episodes = (await Getters.GetEpisodes(project)).OrderBy(e => e.Number);

            // Calculate pages
            // Thanks to petzku and astiob for their contributions to this algorithm
            var pageCount = Math.Ceiling(episodes.Count() / 13d);
            var pageLength = Math.Floor(episodes.Count() / pageCount);
            var roundUp = episodes.Count() % pageCount;

            XDocument? doc = null;
            string anidbError = string.Empty;
            try
            {
                 doc = !string.IsNullOrEmpty(project.AniDBId) ? (await AniDBCache.GetXml(project.AniDBId)) : null;
            }
            catch (Exception e)
            {
                anidbError = T(e.Message, lng);
            }

            // Build the paginator
            var paginator = new LazyPaginatorBuilder()
                .AddUser(Context.User)
                .WithPageFactory((int page0) =>
                {
                    var page1 = page0 + 1;
                    var skip = (int)(page0 * pageLength + Math.Min(page0, roundUp));
                    var length = (int)(pageLength + Math.Min(page1, roundUp));

                    var pagedEpisodes = episodes.Skip(skip).Take(length);
                    var progress = BuildProgress(pagedEpisodes, project, lng, doc, anidbError);

                    // Add the project's MOTD, if applicable
                    if (!string.IsNullOrEmpty(project.Motd))
                        progress = $"{project.Motd}\n{progress}";

                    return new PageBuilder()
                        .WithAuthor(title)
                        .WithTitle(T("title.blameall", lng, page1, pageCount))
                        .WithThumbnailUrl(project.PosterUri)
                        .WithDescription(progress)
                        .WithCurrentTimestamp();
                })
                .WithMaxPageIndex((int)pageCount - 1)
                .AddOption(new Emoji("◀"), PaginatorAction.Backward, ButtonStyle.Secondary)
                .AddOption(new Emoji("▶"), PaginatorAction.Forward, ButtonStyle.Secondary)
                .Build();

            await _interactiveService.SendPaginatorAsync(paginator, interaction, TimeSpan.FromMinutes(2), InteractionResponseType.DeferredChannelMessageWithSource);

            return ExecutionResult.Success;
        }

        private static string BuildProgress(IEnumerable<Episode> pagedEpisodes, Project project, string lng, XDocument? doc, string? anidbError = null)
        {
            StringBuilder sb = new ();
            foreach (var episode in pagedEpisodes)
            {
                sb.Append($"{episode.Number}. ");

                try
                {
                    if (episode.Done)
                        sb.AppendLine($"_{T("blameall.done", lng)}_");
                    else if (episode.Tasks.Any(t => t.Done))
                        sb.AppendLine(StaffList.GenerateProgress(project, episode));
                    else if (doc is not null && !AirDateService.EpisodeAired(doc, episode.Number, project.AirTime ?? "00:00"))
                        sb.AppendLine($"_{T("blameall.notYetAired", lng)}_");
                    else if (!string.IsNullOrEmpty(anidbError))
                        sb.AppendLine(anidbError);
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
