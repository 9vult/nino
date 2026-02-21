// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.Project.Export;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("export", "Export a project")]
    public async Task<RuntimeResult> ExportAsync(string alias)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();

        // Verify project and user - Admin required
        var (userId, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, userId)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var isVerified = await verificationService.VerifyProjectPermissionsAsync(
            projectId,
            userId,
            PermissionsLevel.Administrator
        );
        if (!isVerified)
            return await interaction.FailAsync(T("error.permissions", locale));

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var (exportStatus, json) = await exportHandler.HandleAsync(
            new ExportProjectCommand(projectId, userId)
        );

        if (exportStatus is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                exportStatus switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.projectNotFound", locale),
                    _ => T("error.generic", locale),
                }
            );
        }

        var file = new MemoryStream();
        file.Write(Encoding.UTF8.GetBytes(json!));
        file.Position = 0;

        await interaction.FollowupWithFileAsync(
            file,
            $"{data.Nickname}.json",
            T("project.export", locale, data.Nickname)
        );
        return ExecutionResult.Success;
    }
}
