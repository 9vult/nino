// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;

namespace Nino.Discord.Interactions.Project;

public partial class ProjectModule
{
    [SlashCommand("delete", "Delete a project")]
    public async Task<RuntimeResult> CreateAsync(
        string nickname,
        string anilistId,
        bool isPrivate,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel updateChannel,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel releasesChannel,
        string? title = null,
        ProjectType? type = null,
        [MinValue(1)] int? length = null,
        string? posterUri = null,
        decimal firstEpisode = 1
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        return ExecutionResult.Success;
    }
}
