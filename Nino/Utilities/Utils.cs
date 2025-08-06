using Discord;
using Discord.WebSocket;
using Nino.Records;
using NLog;
using System.Globalization;

namespace Nino.Utilities
{
    internal static class Utils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Verify the given user has administrative permissions
        /// </summary>
        /// <param name="member">Member to check</param>
        /// <param name="guild">Guild to check</param>
        /// <param name="excludeServerAdmins">Whether to exclude Nino server admins</param>
        /// <returns>True if the user is an administrator</returns>
        public static bool VerifyAdministrator(SocketGuildUser member, SocketGuild guild, bool excludeServerAdmins = false)
        {
            // Admin role
            if (member.GuildPermissions.Administrator) return true;

            if (excludeServerAdmins) return false;

            // Nino server-level admin
            if (Nino.DataContext.GetConfig(guild.Id)?.Administrators?.Any(a => a.UserId == member.Id) ?? false)
                return true;

            return false;
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
                if (project.Administrators.Any(a => a.UserId == userId))
                    return true;

                if (Nino.DataContext.GetConfig(project.GuildId)?.Administrators.Any(a => a.UserId == userId) ?? false)
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
        /// Verify the given user has sufficient permissions to use a command
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="project">Project to verify against</param>
        /// <param name="episode">Episode to verify against</param>
        /// <param name="excludeAdmins">Should administrators be excluded?</param>
        /// <returns>True if the user has sufficient permissions</returns>
        public static bool VerifyEpisodeUser(ulong userId, Project project, Episode episode, bool excludeAdmins = false)
        {
            if (project.OwnerId == userId) return true;

            if (!excludeAdmins)
            {
                if (project.Administrators.Any(a => a.UserId == userId))
                    return true;

                if (Nino.DataContext.GetConfig(project.GuildId)?.Administrators?.Any(a => a.UserId == userId) ?? false)
                    return true;
            }
            
            return project.KeyStaff.Any(ks => ks.UserId == userId)
                   || episode.AdditionalStaff.Any(ks => ks.UserId == userId)
                   || episode.PinchHitters.Any(ks => ks.UserId == userId);
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
            if (project.Administrators.Any(a => a.UserId == userId))
                return true;
            if (Nino.DataContext.GetConfig(project.GuildId)?.Administrators?.Any(a => a.UserId == userId) ?? false)
                return true;
            if (episode.PinchHitters.Any(ph => ph.Abbreviation == abbreviation && ph.UserId == userId))
                return true;
            if (project.KeyStaff.Concat(episode.AdditionalStaff).Any(ks => ks.Role.Abbreviation == abbreviation && ks.UserId == userId))
                return true;
            return false;
        }

        /// <summary>
        /// Canonicalize an episode number
        /// </summary>
        /// <param name="input">Raw episode number</param>
        /// <returns>Canonical episode number</returns>
        /// <remarks>If the episode number is not a decimal, it is returned as-is</remarks>
        public static string CanonicalizeEpisodeNumber(string input)
        {
            var trim = input.Trim();
            var replaced = input.Replace(',', '.');
            // If replacing commas with periods results in a decimal, use that. Otherwise, keep the commas.
            return decimal.TryParse(replaced, CultureInfo.InvariantCulture, out var decimalValue) ? decimalValue.ToString(CultureInfo.InvariantCulture) : trim;
        }
        
        /// <summary>
        /// Check if the episode number is a number
        /// </summary>
        /// <param name="input">Raw episode number</param>
        /// <param name="episodeNumber">Output number as a decimal</param>
        /// <returns><see langword="true"/> if the episode number is a number</returns>
        public static bool EpisodeNumberIsNumber(string input, out decimal episodeNumber)
        {
            return decimal.TryParse(input, out episodeNumber);
        }
        
        /// <summary>
        /// Check if the episode number is an integer
        /// </summary>
        /// <param name="input">Raw episode number</param>
        /// <param name="episodeNumber">Output number as an integer</param>
        /// <returns><see langword="true"/> if the episode number is an integer</returns>
        public static bool EpisodeNumberIsInteger(string input, out int episodeNumber)
        {
            return int.TryParse(input, out episodeNumber);
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
                    name = embed.Author?.Name,
                    url = embed.Author?.Url,
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
        /// Alert project owners to an error
        /// </summary>
        /// <param name="exception">Error that occured</param>
        /// <param name="guild">Guild where the error occured</param>
        /// <param name="nickname">Name of the project</param>
        /// <param name="ownerId">ID of the project owner</param>
        /// <param name="location">Where the error occured</param>
        /// <returns>void</returns>
        public static async System.Threading.Tasks.Task AlertError(Exception exception, SocketGuild guild, string nickname, ulong ownerId, string location)
        {
            var message = $"[{location}]: \"{exception.Message}\" from **{guild.Name}** ({guild.Id}), project `{nickname}`.";

            var owner = await Nino.Client.GetUserAsync(ownerId);
            await owner.SendMessageAsync(message);
        }

        /// <summary>
        /// Alert project owners to an error
        /// </summary>
        /// <param name="error">Error that occured</param>
        /// <param name="guild">Guild where the error occured</param>
        /// <param name="nickname">Name of the project</param>
        /// <param name="ownerId">ID of the project owner</param>
        /// <param name="location">Where the error occured</param>
        /// <returns>void</returns>
        public static async System.Threading.Tasks.Task AlertError(string error, SocketGuild guild, string nickname, ulong ownerId, string location)
            => await AlertError(new Exception(error), guild, nickname, ownerId, location);
        
        /// <summary>
        /// The current version of Nino
        /// </summary>
        public static string Version
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
