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
        public static async Task<bool> HandleAirReminderEnable(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Get inputs
            var channelId = ((SocketChannel)subcommand.Options.FirstOrDefault(o => o.Name == "channel")!.Value).Id;
            var roleId = ((SocketRole?)subcommand.Options.FirstOrDefault(o => o.Name == "role")?.Value)?.Id;

            // Set in database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Replace($"/airReminderEnabled", true),
                PatchOperation.Replace($"/airReminderChannelId", channelId.ToString()),
                PatchOperation.Replace($"/airReminderRoleId", roleId?.ToString())
            });

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

            return true;
        }

    }
}
