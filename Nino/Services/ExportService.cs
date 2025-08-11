using System.Text.Json;
using Nino.Records;

namespace Nino.Services
{
    internal static class ExportService
    {
        /// <summary>
        /// Export a project as a JSON stream
        /// </summary>
        /// <param name="project">Project to export</param>
        /// <param name="prettyPrint">Pretty-print the json</param>
        /// <returns></returns>
        public static MemoryStream ExportProject(Project project, bool prettyPrint)
        {
            JsonSerializerOptions options = new()
            {
                IncludeFields = true,
                WriteIndented = prettyPrint,
            };

            var export = new Export { Project = project, Episodes = [.. project.Episodes] };

            var stream = new MemoryStream();
            JsonSerializer.Serialize(stream, export, options: options);
            stream.Position = 0;
            return stream;
        }
    }
}
