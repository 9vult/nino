using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ProjectManagement
    {
        public static async Task<bool> HandleAliasRemove(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            var input = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "alias")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Validate alias exists
            if (!project.Aliases.Any(a => a == input))
                return await Response.Fail(T("error.noSuchAlias", lng, input, project.Nickname), interaction);

            var aliasIndex = Array.IndexOf(project.Aliases, project.Aliases.Single(a => a == input));

            // Remove from database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Remove($"/aliases/{aliasIndex}")
            });

            log.Info($"Removed {input} as an alias from {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.alias.removedAlias", lng, input, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return true;
        }
    }
}
