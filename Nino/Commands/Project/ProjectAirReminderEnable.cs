using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
            [SlashCommand("enable", "Enable airing reminders")]
            public async Task<bool> Enable(
                [Summary("project", "Project nickname")] string alias,
                [Summary("channel", "Channel to post reminders in")] SocketTextChannel channel,
                [Summary("role", "Role to ping for reminders")] SocketRole? role = null
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                var channelId = channel.Id;
                var roleId = role?.Id;

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Set in database
                await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                    patchOperations: [
                        PatchOperation.Replace($"/airReminderEnabled", true),
                        PatchOperation.Replace($"/airReminderChannelId", channelId.ToString()),
                        PatchOperation.Replace($"/airReminderRoleId", roleId?.ToString())
                ]);

                log.Info($"Enabled air reminders for {project.Id}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.airreminder.enabled", lng, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                // Check reminder channel permissions
                if (!PermissionChecker.CheckPermissions(channelId))
                    await Response.Info(T("error.missingChannelPerms", lng, $"<#{channelId}>"), interaction);

                await Cache.RebuildCacheForProject(project.Id);
                return true;
            }
        }
    }
}
