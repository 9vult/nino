using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Blameall(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("blameall", "Check the overall status of a project")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("page", "Page number"), MinValue(1), Autocomplete(typeof(EpisodeAutocompleteHandler))] int pageNumber = 1
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            alias = alias.Trim();
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var episodes = (await Getters.GetEpisodes(project)).OrderBy(e => e.Number);

            // Page size of 12, unless there's 13
            var pageSize = episodes.Count() % 13 == 0 ? 13 : 12;
            var pageCount = Math.Ceiling(episodes.Count() / (double)pageSize);

            if (pageNumber > pageCount)
            {
                Dictionary<string, object> map = new() {
                    ["count"] = pageCount
                };
                return await Response.Fail(T("error.blameall.invalidPageNumber", lng, map, "count"), interaction);
            }

            var pagedEpisodes = episodes.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            
            StringBuilder sb = new();
            foreach (var episode in pagedEpisodes)
            {
                sb.Append($"{episode.Number}. ");

                if (episode.Done)
                    sb.AppendLine($"_{T("blameall.done", lng)}_");
                else if (project.AniDBId != null && !await AirDateService.EpisodeAired(project.AniDBId, episode.Number, project.AirTime ?? "00:00"))
                    sb.AppendLine($"_{T("blameall.notYetAired", lng)}_");
                else
                    sb.AppendLine(StaffList.GenerateProgress(project, episode));
            }
            var progress = sb.ToString();

            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var embed = new EmbedBuilder()
                .WithAuthor(title)
                .WithTitle(T("title.blameall", lng, pageNumber, pageCount))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(progress)
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
