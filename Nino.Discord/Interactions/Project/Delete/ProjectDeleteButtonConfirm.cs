// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [ComponentInteraction("nino:project:delete:confirm:*:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> ConfirmDeleteAsync(string projectId, string userId)
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        var embed = new EmbedBuilder()
            .WithAuthor("Project Name")
            .WithTitle("Are you sure you want to delete this project?")
            .WithDescription($"Confirmed deletion of {projectId} {userId}")
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = null;
        });

        return ExecutionResult.Success;
    }
}
