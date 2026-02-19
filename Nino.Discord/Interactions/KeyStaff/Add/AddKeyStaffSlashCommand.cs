// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Enums;
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

        // Verify project and user - Administrator required
        var (userId, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (status, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, userId)
        );

        if (status is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        return ExecutionResult.Success;
    }
}
