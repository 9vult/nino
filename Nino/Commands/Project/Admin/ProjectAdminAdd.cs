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
        public partial class Admin
        {
            [SlashCommand("add", "Add an administrator to this project")]
            public async Task<RuntimeResult> Add(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("member", "Staff member")] SocketUser member
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                var memberId = member.Id;
                var staffMention = $"<@{memberId}>";

                // Verify project and user - Owner required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate user isn't already an admin
                if (project.AdministratorIds.Any(a => a == memberId))
                    return await Response.Fail(T("error.admin.alreadyAdmin", lng, staffMention), interaction);

                // Add to database
                await AzureHelper.PatchProjectAsync(project, [
                    PatchOperation.Add("/administratorIds/-", memberId.ToString())
                ]);

                Log.Info($"Added M[{memberId} (@{member.Username})] as an administrator for {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.admin.added", lng, staffMention, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(project.Id);
                return ExecutionResult.Success;
            }
        }
    }
}
