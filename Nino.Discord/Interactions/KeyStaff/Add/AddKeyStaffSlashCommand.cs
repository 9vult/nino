// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.Add;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [SlashCommand("add", "Add a new Key Staff to the whole project")]
    public async Task<RuntimeResult> AddAsync(
        string alias,
        SocketUser member,
        [MaxLength(16)] string abbreviation,
        [MaxLength(32)] string fullName,
        bool isPseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var memberId = await identityService.GetOrCreateUserByDiscordIdAsync(
            member.Id,
            member.Username
        );

        var result = await addHandler.HandleAsync(
            new AddKeyStaffCommand(
                projectId,
                memberId,
                abbreviation,
                fullName,
                isPseudo,
                requestedBy
            )
        );

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.projectNotFound", locale),
                    ResultStatus.Conflict => T("task.creation.conflict", locale, abbreviation),
                    _ => T("error.generic", locale),
                }
            );
        }

        return ExecutionResult.Success;
    }
}
