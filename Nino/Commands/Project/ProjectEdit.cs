using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using System.Text.RegularExpressions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [GeneratedRegex(@"^([0-9]{2}):([0-9]{2})$")]
        private static partial Regex Time();
        [GeneratedRegex(@"^(true|false|yes|no)$")]
        private static partial Regex Bool();
        [GeneratedRegex(@"^(true|yes)$")]
        private static partial Regex Truthy();

        [SlashCommand("edit", "Edit a project")]
        public async Task<RuntimeResult> Edit(
            [Summary("project", "Project nickname")] string alias,
            [Summary("option", "Option to change")] ProjectEditOption option,
            [Summary("newvalue", "New value")] string newValue
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            newValue = newValue.Trim();

            // Verify project and user - Owner required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            string helperText = string.Empty;
            PatchOperation operation;

            switch (option)
            {
                case ProjectEditOption.Title:
                    operation = PatchOperation.Replace($"/title", newValue);
                    break;

                case ProjectEditOption.Poster:
                    operation = PatchOperation.Replace($"/posterUri", newValue);
                    break;

                case ProjectEditOption.MOTD:
                    var motd = newValue == "-" ? null : newValue;
                    operation = PatchOperation.Replace($"/motd", motd);
                    helperText = T("info.resettable", lng);
                    break;

                case ProjectEditOption.AniDBId:
                    operation = PatchOperation.Replace($"/aniDBId", newValue);
                    break;

                case ProjectEditOption.AirTime24h:
                    if (!Time().IsMatch(newValue))
                        return await Response.Fail(T("error.incorrectAirTimeFormat", lng), interaction);
                    operation = PatchOperation.Replace($"/airTime", newValue);
                    break;

                case ProjectEditOption.IsPrivate:
                    var input = newValue.ToLowerInvariant();
                    if (!Bool().IsMatch(input))
                        return await Response.Fail(T("error.incorrectBooleanFormat", lng), interaction);
                    operation = PatchOperation.Replace($"/isPrivate", Truthy().IsMatch(input));
                    break;

                case ProjectEditOption.UpdateChannelID:
                    if (!ulong.TryParse(newValue.Replace("<#", "").Replace(">", "").Trim(), out var updateChannelId)
                        || Nino.Client.GetChannel(updateChannelId) == null)
                        return await Response.Fail(T("error.noSuchChannel", lng), interaction);
                    operation = PatchOperation.Replace($"/updateChannelId", updateChannelId.ToString());
                    if (!PermissionChecker.CheckPermissions(updateChannelId))
                        await Response.Info(T("error.missingChannelPerms", lng, $"<#{updateChannelId}>"), interaction);
                    break;

                case ProjectEditOption.ReleaseChannelID:
                    if (!ulong.TryParse(newValue.Replace("<#", "").Replace(">", "").Trim(), out var releaseChannelId)
                        || Nino.Client.GetChannel(releaseChannelId) == null)
                        return await Response.Fail(T("error.noSuchChannel", lng), interaction);
                    operation = PatchOperation.Replace($"/releaseChannelId", releaseChannelId.ToString());
                    if (!PermissionChecker.CheckReleasePermissions(releaseChannelId))
                        await Response.Info(T("error.missingChannelPermsRelease", lng, $"<#{releaseChannelId}>"), interaction);
                    break;

                default:
                    return await Response.Fail(T("error.generic", lng), interaction);
            }

            await AzureHelper.Projects!.PatchItemAsync<Project>(
                id: project.Id,
                partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[] { operation }
            );

            log.Info($"Updated project {project.Id} {option.ToFriendlyString()} to {newValue}");

            var embedDescription = T("project.edited", lng, project.Nickname, option.ToFriendlyString());
            if (!string.IsNullOrEmpty(helperText))
                embedDescription += $"\n{helperText}";

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(embedDescription)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);

            return ExecutionResult.Success;
        }
    }
}
