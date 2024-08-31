using Discord;
using Discord.WebSocket;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Done
    {
        public static async Task<bool> Handle(SocketSlashCommand interaction, Project project, string abbreviation)
        {
            var lng = interaction.UserLocale;

            var episodes = Cache.GetEpisodes(project.Id);

            // Find the episode the team is working on
            var nextWorkingEpisode = episodes.FirstOrDefault(e => !e.Done)?.Number ?? episodes.LastOrDefault()?.Number;
            if (nextWorkingEpisode == null)
                return await Response.Fail(T("error.noIncompleteEpisodes", lng), interaction);

            // Find the next episode awaiting this task's completion
            var nextTaskEpisode = episodes.FirstOrDefault(e => e.Tasks.Any(t => t.Abbreviation == abbreviation && !t.Done))?.Number;
            if (nextTaskEpisode == null)
            {
                // We do a little research
                if (episodes.Any(e => e.Tasks.Any(t => t.Abbreviation == abbreviation)))
                    return await Response.Fail(T("error.taskCompleteAllEpisodes", lng), interaction);
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);
            }
            
            // Are they the same? (Easy)
            if (nextTaskEpisode == nextWorkingEpisode)
            {

            }
            // Working ahead time
            else
            {

            }

            return true;     
        }
    }
}
