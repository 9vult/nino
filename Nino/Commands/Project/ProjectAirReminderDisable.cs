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
        public partial class AirReminder
        {
            [SlashCommand("disable", "Disable airing reminders")]
            public async Task<RuntimeResult> Disable(
                [Summary("project", "Project nickname")] string alias
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Set in database
                await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                    patchOperations: [
                        PatchOperation.Replace($"/airReminderEnabled", false),
                        PatchOperation.Replace<string?>($"/airReminderChannelId", null),
                        PatchOperation.Replace<string?>($"/airReminderRoleId", null)
                ]);

                log.Info($"Disabled air reminders for {project.Id}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.airreminder.disabled", lng, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(project.Id);
                return ExecutionResult.Success;
            }
        }
    }
}
