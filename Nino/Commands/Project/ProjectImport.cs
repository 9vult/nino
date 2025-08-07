using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;
using Discord.Interactions;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("import", "Import a project from JSON")]
        public async Task<RuntimeResult> Import(
            [Summary("file", "Project Template")] IAttachment file,
            [Summary("updateChannel", "Channel to post updates to"), ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel updateChannel,
            [Summary("releaseChannel", "Channel to post releases to"), ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel releaseChannel
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            
            var updateChannelId = updateChannel.Id;
            var releaseChannelId = releaseChannel.Id;
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);

            Log.Info($"Project import requested by M[{interaction.User.Id} (@{interaction.User.Username})]");

            Export? import;
            
            try
            {
                Log.Trace($"Attempting to get and parse JSON...");
                using var client = new HttpClient();
                import = await client.GetFromJsonAsync<Export>(file.Url, new JsonSerializerOptions
                {
                    IncludeFields = true,
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });

                if (import is null)
                {
                    Log.Trace($"Project import failed (null)");
                    return await Response.Fail(T("error.generic", lng), interaction);
                }
                Log.Trace($"Getting and parsing JSON successful!");
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Trace($"Project creation from json file failed");
                return await Response.Fail(e.Message, interaction);
            }
            
            // Sanitize input
            import.Project.Nickname = import.Project.Nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty); // remove spaces

            // Verify data
            if (db.Projects.Any(p => p.GuildId == guildId && p.Nickname == import.Project.Nickname))
                return await Response.Fail(T("error.project.nameInUse", lng, import.Project.Nickname), interaction);
            
            if (!Uri.TryCreate(import.Project.PosterUri, UriKind.Absolute, out _))
                return await Response.Fail(T("error.project.invalidPosterUrl", lng), interaction);
            
            // Set IDs
            import.Project.GuildId = guildId;
            import.Project.OwnerId = interaction.User.Id;
            import.Project.Id = Guid.NewGuid();
            
            import.Project.UpdateChannelId = updateChannelId;
            import.Project.ReleaseChannelId = releaseChannelId;
            
            // Disable potentially problematic settings
            import.Project.Administrators = [];
            import.Project.CongaReminderChannelId = null;
            import.Project.AirReminderChannelId = null;
            import.Project.CongaReminderEnabled = false;
            import.Project.AirReminderEnabled = false;
            
            foreach (var staff in import.Project.KeyStaff)
                staff.Id = Guid.Empty;
            
            foreach (var episode in import.Episodes)
            {
                episode.GuildId = guildId;
                episode.ProjectId = import.Project.Id;
                episode.Id = Guid.Empty;
                
                foreach (var ph in episode.PinchHitters)
                    ph.Id = Guid.Empty;
                foreach (var task in episode.Tasks)
                    task.Id = Guid.Empty;
                foreach (var staff in episode.AdditionalStaff)
                    staff.Id = Guid.Empty;
            }
            
            Log.Info($"Creating project {import.Project} for M[{import.Project.OwnerId} (@{member.Username})] from JSON file '{file.Filename}' with {import.Episodes.Length} episodes");

            // Add project and episodes to database
            await db.Projects.AddAsync(import.Project);
            await db.Episodes.AddRangeAsync(import.Episodes);
            
            // Create configuration if the guild doesn't have one yet
            if (db.GetConfig(guildId) == null)
            {
                Log.Info($"Creating default configuration for guild {guildId}");
                await db.Configurations.AddAsync(Configuration.CreateDefault(guildId));
            }

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("project.imported", lng, import.Project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            // Check progress channel permissions
            if (!PermissionChecker.CheckPermissions(updateChannelId))
                await Response.Info(T("error.missingChannelPerms", lng, $"<#{updateChannelId}>"), interaction);
            if (!PermissionChecker.CheckReleasePermissions(releaseChannelId))
                await Response.Info(T("error.missingChannelPermsRelease", lng, $"<#{releaseChannelId}>"), interaction);

            await db.SaveChangesAsync();
            return ExecutionResult.Success;
        }
    }
}
