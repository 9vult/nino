using Discord;
using Discord.Interactions;
using Nino.Handlers;
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
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("option", "Option to change")] ProjectEditOption option,
            [Summary("newValue", "New value")] string newValue
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            newValue = newValue.Trim();

            // Verify project and user - Owner or Admin required
            var project = db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            if (project.IsArchived)
                return await Response.Fail(T("error.archived", lng), interaction);

            var helperText = string.Empty;

            switch (option)
            {
                case ProjectEditOption.Title:
                    project.Title = newValue;
                    break;

                case ProjectEditOption.Poster:
                    if (!Uri.TryCreate(newValue, UriKind.Absolute, out Uri? _))
                        return await Response.Fail(T("error.project.invalidPosterUrl", lng), interaction);

                    project.PosterUri = newValue;
                    break;

                case ProjectEditOption.MOTD:
                    var motd = newValue == "-" ? null : newValue;
                    project.Motd = motd;
                    helperText = T("info.resettable", lng);
                    break;

                case ProjectEditOption.AniListId:
                    var ok = int.TryParse(newValue, out var id);
                    if (ok)
                        project.AniListId = id;
                    else
                        return await Response.Fail(T("error.incorrectIntegerFormat", lng), interaction);
                    break;
                
                case ProjectEditOption.AniListOffset:
                    ok = int.TryParse(newValue, out var offset);
                    if (ok)
                        project.AniListOffset = offset;
                    else
                        return await Response.Fail(T("error.incorrectIntegerFormat", lng), interaction);
                    break;

                case ProjectEditOption.IsPrivate:
                    var input = newValue.ToLowerInvariant();
                    if (!Bool().IsMatch(input))
                        return await Response.Fail(T("error.incorrectBooleanFormat", lng), interaction);
                    project.IsPrivate = Truthy().IsMatch(input);
                    break;

                case ProjectEditOption.UpdateChannelID:
                    if (!ulong.TryParse(newValue.Replace("<#", "").Replace(">", "").Trim(), out var updateChannelId)
                        || Nino.Client.GetChannel(updateChannelId) == null)
                        return await Response.Fail(T("error.noSuchChannel", lng), interaction);
                    project.UpdateChannelId = updateChannelId;
                    if (!PermissionChecker.CheckPermissions(updateChannelId))
                        await Response.Info(T("error.missingChannelPerms", lng, $"<#{updateChannelId}>"), interaction);
                    break;

                case ProjectEditOption.ReleaseChannelID:
                    if (!ulong.TryParse(newValue.Replace("<#", "").Replace(">", "").Trim(), out var releaseChannelId)
                        || Nino.Client.GetChannel(releaseChannelId) == null)
                        return await Response.Fail(T("error.noSuchChannel", lng), interaction);
                    project.ReleaseChannelId = releaseChannelId;
                    if (!PermissionChecker.CheckReleasePermissions(releaseChannelId))
                        await Response.Info(T("error.missingChannelPermsRelease", lng, $"<#{releaseChannelId}>"), interaction);
                    break;
                
                case ProjectEditOption.Nickname:
                    // Sanitize input
                    newValue = newValue.Trim().ToLowerInvariant().Replace(" ", string.Empty);
                    
                    // Verify data
                    var guildId = interaction.GuildId;
                    if (db.Projects.Any(p => p.GuildId == guildId && p.Nickname == newValue))
                        return await Response.Fail(T("error.project.nameInUse", lng, newValue), interaction);
                    
                    Log.Info($"Changing nickname of {project} to {newValue}");
                    project.Nickname = newValue;
                    break;
                
                default:
                    return await Response.Fail(T("error.generic", lng), interaction);
            }

            Log.Info($"Updated project {project} {option.ToFriendlyString(lng)} to {newValue}");

            var embedDescription = T("project.edited", lng, project.Nickname, option.ToFriendlyString(lng));
            if (!string.IsNullOrEmpty(helperText))
                embedDescription += $"\n{helperText}";

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(embedDescription)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
