using Discord;
using Discord.Interactions;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("transfer-server", "Transfer a project from another server to here")]
        public async Task<RuntimeResult> TransferServer(
            [Summary("serverId", "ID of the server the project is currently in")] string serverId,
            [Summary("project", "Project nickname")] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            var originGuildIdStr = serverId.Trim();
            alias = alias.Trim();
            var newGuildId = interaction.GuildId ?? 0;

            // Check for guild administrator status
            var guild = Nino.Client.GetGuild(newGuildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(db, member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);

            // Validate origin server
            if (!ulong.TryParse(originGuildIdStr, out var oldGuildId))
                return await Response.Fail(T("error.invalidServerId", lng), interaction);
            var originGuild = Nino.Client.GetGuild(oldGuildId);
            if (originGuild == null)
                return await Response.Fail(T("error.noSuchServer", lng), interaction);

            // Verify project and user - Owner required
            var project = await db.ResolveAlias(alias, interaction, observingGuildId: oldGuildId);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!project.VerifyUser(db, interaction.User.Id, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Modify the data
            project.GuildId = newGuildId;
            foreach (var episode in project.Episodes)
            {
                episode.GuildId = newGuildId;
            }
            
            Log.Info($"Transfered project {project} from server {oldGuildId} to new server {newGuildId}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.server.transferred", lng, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
