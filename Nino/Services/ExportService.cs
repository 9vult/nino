using Nino.Records;
using Nino.Utilities;
using System.Text.Json;

namespace Nino.Services
{
    internal static class ExportService
    {
        /// <summary>
        /// Export a project as a JSON stream
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<MemoryStream> ExportProject(Project project, bool prettyPrint)
        {
            JsonSerializerOptions options = new() { IncludeFields = true, WriteIndented = prettyPrint };

            var episodes = await Getters.GetEpisodes(project);

            var export = new Export
            {
                Project = project,
                Episodes = [.. episodes]
            };
            
            var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, export, options: options);
            stream.Position = 0;
            return stream;
        }
    }
}
