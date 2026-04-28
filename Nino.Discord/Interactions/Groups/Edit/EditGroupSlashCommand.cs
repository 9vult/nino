// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Groups.Edit;
using Nino.Core.Features.Queries.Groups.GetGenericData;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Discord.Interactions.Groups;

public partial class GroupModule
{
    [SlashCommand("edit", "Edit group configuration")]
    public async Task<RuntimeResult> EditAsync(
        [Autocomplete(typeof(LocaleAutocompleteHandler))] Locale? locale = null,
        bool? publishPrivateProgress = null,
        ProgressResponseType? progressResponseType = null,
        ProgressPublishType? progressPublishType = null,
        CongaPrefixType? congaPrefixType = null,
        string? releasePrefix = null,
        string? name = null
    )
    {
        var interaction = Context.Interaction;
        var interactionLocale = interaction.UserLocale;

        // Cleanup
        releasePrefix = releasePrefix?.Trim();
        name = name?.Trim();

        if (releasePrefix == "-")
            releasePrefix = string.Empty;

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var isDiscordAdmin = member.GuildPermissions.Administrator;

        var command = new EditGroupCommand(
            GroupId: groupId,
            RequestedBy: requestedBy,
            OverrideVerification: isDiscordAdmin,
            Locale: locale,
            PublishPrivateProgress: publishPrivateProgress,
            ProgressResponseType: progressResponseType,
            ProgressPublishType: progressPublishType,
            CongaPrefixType: congaPrefixType,
            ReleasePrefix: releasePrefix,
            Name: name
        );

        var result = await editHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getGenericDataHandler.HandleAsync(new GetGenericGroupDataQuery(groupId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                interactionLocale,
                new FailureContext()
            );
        }

        var gData = result.Value;

        var body = new StringBuilder();
        body.AppendLine(T("group.edit.success", interactionLocale));

        if (!string.IsNullOrEmpty(releasePrefix))
        {
            body.AppendLine();
            body.AppendLine(T("group.edit.clearable", interactionLocale));
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithAuthor(gData.GroupName)
            .WithTitle(T("group.configuration.title", interactionLocale))
            .WithDescription(body.ToString())
            .WithCurrentTimestamp()
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
