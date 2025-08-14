using System.Text;
using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Localizer;
using NaturalSort.Extension;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands;

public class BlameAll(DataContext db, InteractiveService interactive)
    : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("blameall", "Check the overall status of a project")]
    public async Task<RuntimeResult> Handle(
        [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
        BlameAllFilter filter = BlameAllFilter.All,
        BlameAllType type = BlameAllType.Normal,
        bool includePseudo = false
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;
        var gLng =
            db.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale()
            ?? interaction.GuildLocale
            ?? "en-US";
        alias = alias.Trim();

        // Verify project
        var project = await db.ResolveAlias(alias, interaction, includeObservers: true);
        if (project is null)
            return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

        // Restrict display of pseudo-tasks to Key Staff
        if (includePseudo && !project.VerifyUser(db, interaction.User.Id, includeStaff: true))
            includePseudo = false;

        Log.Trace(
            $"Blame All-ing {project} for M[{interaction.User.Id} (@{interaction.User.Username})]"
        );

        var title = project.IsPrivate
            ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
            : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

        var episodes = filter switch
        {
            BlameAllFilter.All => project
                .Episodes.OrderBy(
                    e => e.Number,
                    StringComparison.OrdinalIgnoreCase.WithNaturalSort()
                )
                .ToList(),
            BlameAllFilter.InProgress => project
                .Episodes.Where(e => !e.Done && e.Tasks.Any(t => t.Done))
                .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                .ToList(),
            BlameAllFilter.Incomplete => project
                .Episodes.Where(e => !e.Done)
                .OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
                .ToList(),
            _ => project
                .Episodes.OrderBy(
                    e => e.Number,
                    StringComparison.OrdinalIgnoreCase.WithNaturalSort()
                )
                .ToList(), // All
        };

        // Calculate pages
        // Thanks to petzku and astiob for their contributions to this algorithm
        var pageCount = Math.Ceiling(episodes.Count / 13d);
        var pageLength = Math.Floor(episodes.Count / pageCount);
        var roundUp = episodes.Count % pageCount;

        // Build the paginator
        var paginator = new LazyPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageFactory(async page0 =>
            {
                var page1 = page0 + 1;
                var skip = (int)(page0 * pageLength + Math.Min(page0, roundUp));
                var length = (int)(pageLength + (page1 <= roundUp ? 1 : 0));

                var pagedEpisodes = episodes.Skip(skip).Take(length);
                var progress = await BuildProgress(
                    pagedEpisodes,
                    project,
                    lng,
                    type,
                    !includePseudo
                );

                // Add the project's MOTD or archival notice, if applicable
                if (!string.IsNullOrEmpty(project.Motd))
                    progress = $"{project.Motd}\n{progress}";
                if (project.IsArchived)
                    progress = $"{T("blame.archived", lng)}\n{progress}";

                return new PageBuilder()
                    .WithAuthor(title, url: project.AniListUrl)
                    .WithTitle(T("title.blameall", lng))
                    .WithThumbnailUrl(project.PosterUri)
                    .WithDescription(progress)
                    .WithCurrentTimestamp();
            })
            .WithMaxPageIndex((int)pageCount - 1)
            .AddOption(new Emoji("◀"), PaginatorAction.Backward, ButtonStyle.Secondary)
            .AddOption(new Emoji("▶"), PaginatorAction.Forward, ButtonStyle.Secondary)
            .WithActionOnTimeout(ActionOnStop.DeleteInput)
            .WithRestrictedPageFactory(users =>
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

        await interactive.SendPaginatorAsync(
            paginator,
            interaction,
            TimeSpan.FromMinutes(1),
            InteractionResponseType.DeferredChannelMessageWithSource
        );

        return ExecutionResult.Success;
    }

    private static async Task<string> BuildProgress(
        IEnumerable<Episode> pagedEpisodes,
        Project project,
        string lng,
        BlameAllType type,
        bool excludePseudo
    )
    {
        StringBuilder sb = new();
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
                        sb.AppendLine(episode.GenerateProgress(excludePseudo: excludePseudo));
                    if (type == BlameAllType.StallCheck)
                        sb.AppendLine(
                            T(
                                "episode.lastUpdated",
                                lng,
                                $"<t:{episode.Updated?.ToUnixTimeSeconds()}:R>"
                            )
                        );
                }
                else if (
                    project.AniListId is not null
                    && Episode.EpisodeNumberIsNumber(episode.Number, out var decimalNumber)
                    && await AirDateService.EpisodeAired(
                        (int)project.AniListId,
                        decimalNumber + (project.AniListOffset ?? 0)
                    )
                        is false
                )
                {
                    if (type == BlameAllType.Normal)
                        sb.AppendLine($"_{T("blameall.notYetAired", lng)}_");
                    if (type == BlameAllType.StallCheck)
                        sb.AppendLine(
                            await AirDateService.GetAirDateString(
                                (int)project.AniListId,
                                decimalNumber + (project.AniListOffset ?? 0),
                                lng
                            )
                        );
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
