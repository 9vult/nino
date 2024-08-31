using Discord;
using Discord.WebSocket;
using Nino.Records;

namespace Nino.Utilities
{
    internal static class Utils
    {
        /// <summary>
        /// Resolve an alias to its project
        /// </summary>
        /// <param name="query">Alias to resolve</param>
        /// <param name="interaction">Interaction requesting resolution</param>
        /// <param name="observingGuildId">ID of the build being observed, if applicable</param>
        /// <returns>Project the alias references to, or null</returns>
        public static Project? ResolveAlias(string query, SocketInteraction interaction, ulong? observingGuildId = null)
        {
            var guildId = observingGuildId ?? interaction.GuildId ?? 0;
            var cache = Cache.GetProjects(guildId);
            if (cache == null) return null;

            return cache.Where(p => string.Equals(p.Nickname, query, StringComparison.InvariantCultureIgnoreCase) 
                || p.Aliases.Any(a => string.Equals(a, query, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
        }

        /// <summary>
        /// Verify the given user has sufficient permissions to use a command
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="project">Project to verify against</param>
        /// <param name="excludeAdmins">Should administrators be excluded?</param>
        /// <param name="includeKeyStaff">Should Key Staff be included?</param>
        /// <returns>True if the user has sufficient permissions</returns>
        public static bool VerifyUser(ulong userId, Project project, bool excludeAdmins = false, bool includeKeyStaff = false)
        {
            if (project.OwnerId == userId) return true;

            if (!excludeAdmins)
            {
                if (project.AdministratorIds.Any(a => a == userId))
                    return true;

                if (Cache.GetConfig(project.GuildId)?.AdministratorIds?.Any(a => a == userId) ?? false)
                    return true;
            }
            if (includeKeyStaff)
            {
                if (project.KeyStaff.Any(ks => ks.UserId == userId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Verify a user has sufficient permissions to make progress on a task
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="project">Project to check</param>
        /// <param name="episode">Episode to check</param>
        /// <param name="abbreviation">Abbreviation to check</param>
        /// <returns>True if the user has permission</returns>
        public static bool VerifyTaskUser(ulong userId, Project project, Episode episode, string abbreviation)
        {
            if (project.OwnerId == userId)
                return true;
            if (project.AdministratorIds.Any(a => a == userId))
                return true;
            if (Cache.GetConfig(project.GuildId)?.AdministratorIds?.Any(a => a == userId) ?? false)
                return true;
            if (project.KeyStaff.Concat(episode.AdditionalStaff).Any(ks => ks.Role.Abbreviation == abbreviation && ks.UserId == userId))
                return true;
            return false;
        }

        /// <summary>
        /// Convert an Embed to a simple object for JSON serialization
        /// </summary>
        /// <param name="embed">Embed to convert</param>
        /// <returns>Simple object for JSON serialization</returns>
        public static object EmbedToJsonObject(Embed embed)
        {
            return new
            {
                author = new {
                    name = embed.Author?.Name
                },
                title = embed.Title,
                description = embed.Description,
                thumbnail = new {
                    url = embed.Thumbnail?.Url
                },
                timestamp = embed.Timestamp?.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz")
            };
        }

        /// <summary>
        /// The current version of Nino
        /// </summary>
        public static string VERSION
        {
            get
            {
                var version = (!ThisAssembly.Git.SemVer.Major.Equals(string.Empty))
                    ? $"v{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}{ThisAssembly.Git.SemVer.DashLabel} "
                    : "";
                var position = $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}";
                return $"{version}@ {position}";
            }
        }
    }
}
