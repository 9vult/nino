// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.Remove;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [SlashCommand("remove", "Remove a Key Staff from the whole project")]
    public async Task<RuntimeResult> RemoveAsync(string alias, [MaxLength(16)] string abbreviation)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        abbreviation = abbreviation.Trim();

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var result = await removeHandler.HandleAsync(
            new RemoveKeyStaffCommand(projectId, abbreviation, requestedBy)
        );

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("keyStaff.notFound", locale),
                    _ => T("error.generic", locale),
                }
            );
        }

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var body = new StringBuilder();
        body.AppendLine(T("keyStaff.delete.success", locale, abbreviation));

        if (result.Value?.CompletedEpisodeNumbers.Count > 0)
        {
            var dict = new Dictionary<string, object>
            {
                ["number"] = result.Value.CompletedEpisodeNumbers.Count,
            };
            body.AppendLine(T("keyStaff.deleted.completedEpisodes", locale, dict));
        }

        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
