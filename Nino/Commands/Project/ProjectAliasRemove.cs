using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Alias
        {
            [SlashCommand("remove", "Remove an alias")]
            public async Task<bool> Remove(
                [Summary("project", "Project nickname")] string alias,
                [Summary("alias", "Alias")] string input
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                input = input.Trim();

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
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
                    patchOperations: [
                        PatchOperation.Remove($"/aliases/{aliasIndex}")
                ]);

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
}
