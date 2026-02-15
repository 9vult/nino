// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("delete", "Delete a project")]
    public async Task<RuntimeResult> DeleteAsync(string alias)
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        // Verify project and user - Owner required
        var projectId = Guid.NewGuid();
        var userId = interaction.User.Id;

        // Ask if the user is sure
        var embed = new EmbedBuilder()
            .WithAuthor("Project Name")
            .WithTitle("❓ Are you sure you want to delete this project?")
            .WithDescription("The impostor is suspicious!")
            .WithCurrentTimestamp()
            .Build();

        var cancelId = $"nino:project:delete:cancel:{projectId}:{userId}";
        var confirmId = $"nino:project:delete:confirm:{projectId}:{userId}";

        var component = new ComponentBuilder()
            .WithButton("Cancel", cancelId, ButtonStyle.Danger)
            .WithButton("Confirm", confirmId, ButtonStyle.Secondary)
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = component;
        });

        return ExecutionResult.Success;
    }
}
