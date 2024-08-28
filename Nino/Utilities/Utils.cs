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
        public static async Task<Project?> ResolveAlias(string query, SocketInteraction interaction, ulong? observingGuildId = null)
        {
            var cachedProject = ResolveCachedAlias(query, interaction, observingGuildId);

            if (cachedProject != null)
                return await Getters.GetProject(cachedProject.Id, cachedProject.GuildId);
            else
                return null;
        }

        /// <summary>
        /// Resolve an alias to its cached project
        /// </summary>
        /// <param name="query">Alias to resolve</param>
        /// <param name="interaction">Interaction requesting resolution</param>
        /// <param name="observingGuildId">ID of the build being observed, if applicable</param>
        /// <returns>CachedProject the alias references to, or null</returns>
        public static Project? ResolveCachedAlias(string query, SocketInteraction interaction, ulong? observingGuildId = null)
        {
            var guildId = observingGuildId ?? interaction.GuildId ?? 0;
            var cache = Cache.GetProjects(guildId);
            if (cache == null) return null;

            return cache.Where(p => p.Nickname == query || p.Aliases.Any(a => a == query)).FirstOrDefault();
        }

        /// <summary>
        /// Verify the given user has sufficient permissions to perform a task
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="project">Project to verify against</param>
        /// <param name="excludeAdmins">Should administrators be excluded?</param>
        /// <returns>True if the user has sufficient permissions</returns>
        public static bool VerifyUser(ulong userId, Project project, bool excludeAdmins = false)
        {
            if (project.OwnerId == userId) return true;

            if (!excludeAdmins)
            {
                if (project.AdministratorIds.Any(a => a == userId))
                    return true;

                if (Cache.GetConfig(project.GuildId)?.AdministratorIds?.Any(a => a == userId) ?? false)
                    return true;
            }

            return false;
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
