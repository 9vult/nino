using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("transferownership", "Transfer project ownership to someone else")]
        public async Task<RuntimeResult> Delete(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("member", "Staff member")] SocketUser member
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Verify project and user - Owner required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Get inputs
            var memberId = member.Id;

            // Swap in database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: [
                    PatchOperation.Replace($"/ownerId", memberId.ToString())
            ]);

            log.Info($"Transfered project ownership of {project.Id} to {memberId}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.owner.transferred", lng, staffMention, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
