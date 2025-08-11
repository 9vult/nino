using Discord;
using Discord.WebSocket;
using Nino.Utilities.Extensions;
using NLog;

namespace Nino.Utilities
{
    internal static class Utils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Verify the given user has administrative permissions
        /// </summary>
        /// <param name="db">DataContext to use</param>
        /// <param name="member">Member to check</param>
        /// <param name="guild">Guild to check</param>
        /// <param name="excludeServerAdmins">Whether to exclude Nino server admins</param>
        /// <returns>True if the user is an administrator</returns>
        public static bool VerifyAdministrator(
            DataContext db,
            SocketGuildUser member,
            SocketGuild guild,
            bool excludeServerAdmins = false
        )
        {
            // Admin role
            if (member.GuildPermissions.Administrator)
                return true;

            if (excludeServerAdmins)
                return false;

            // Nino server-level admin
            if (db.GetConfig(guild.Id)?.Administrators.Any(a => a.UserId == member.Id) ?? false)
                return true;

            return false;
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
        public static async Task AlertError(
            Exception exception,
            SocketGuild guild,
            string nickname,
            ulong ownerId,
            string location
        )
        {
            var message =
                $"[{location}]: \"{exception.Message}\" from **{guild.Name}** ({guild.Id}), project `{nickname}`.";

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
        public static async Task AlertError(
            string error,
            SocketGuild guild,
            string nickname,
            ulong ownerId,
            string location
        ) => await AlertError(new Exception(error), guild, nickname, ownerId, location);

        /// <summary>
        /// The current version of Nino
        /// </summary>
        public static string Version
        {
            get
            {
                var version =
                    (!ThisAssembly.Git.SemVer.Major.Equals(string.Empty))
                        ? $"v{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}{ThisAssembly.Git.SemVer.DashLabel} "
                        : "";
                const string position = $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}";
                return $"{version}@ {position}";
            }
        }
    }
}
