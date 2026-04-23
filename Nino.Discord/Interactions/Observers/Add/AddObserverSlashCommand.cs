// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Observers.Add;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Observers;

public partial class ObserverModule
{
    [SlashCommand("add", "Add an observer to a project")]
    public async Task<RuntimeResult> AddAsync(
        [Summary(name: "server-id")] string rawServerId,
        [MaxLength(Length.Alias), Autocomplete(typeof(ObserverProjectAutocompleteHandler))]
            Alias alias,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel updateChannel,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel releaseChannel,
        SocketRole? primaryRole = null,
        SocketRole? secondaryRole = null,
        SocketRole? tertiaryRole = null
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        rawServerId = rawServerId.Trim();

        if (!ulong.TryParse(rawServerId, out var serverId))
            return await interaction.FailAsync(T("observer.invalidServerId", locale, rawServerId));

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var originGroupId = await identityService.GetGroupByDiscordIdAsync(serverId);

        if (originGroupId is null)
            return await interaction.FailAsync(T("observer.unknownGroup", locale, serverId));

        var resolve = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, originGroupId.Value, requestedBy)
        );

        if (!resolve.IsSuccess)
        {
            return await interaction.FailAsync(
                resolve.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectId = resolve.Value;

        var updateChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(
            updateChannel.Id
        );
        var releaseChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(
            releaseChannel.Id
        );

        var primaryRoleId = primaryRole is not null
            ? (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(primaryRole.Id)
            : null;
        var secondaryRoleId = secondaryRole is not null
            ? (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(secondaryRole.Id)
            : null;
        var tertiaryRoleId = tertiaryRole is not null
            ? (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(tertiaryRole.Id)
            : null;

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);
        var isDiscordAdmin = member.GuildPermissions.Administrator;

        var command = new AddObserverCommand(
            projectId,
            groupId,
            requestedBy,
            isDiscordAdmin,
            updateChannelId,
            releaseChannelId,
            primaryRoleId,
            secondaryRoleId,
            tertiaryRoleId
        );

        var result = await addHandler
            .HandleAsync(command)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            );

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(
                result.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var pData = result.Value;

        // Check bot permissions in specified channels
        var canUseUpdateChannel = botPermissionsService.HasMessagePermissions(updateChannel.Id);
        var canUseReleaseChannel = botPermissionsService.HasMessagePermissions(releaseChannel.Id);

        // Build success embed body
        var body = new StringBuilder();
        body.AppendLine(T("observer.creation.success", locale));

        if (!canUseUpdateChannel || !canUseReleaseChannel)
        {
            body.AppendLine();
            body.AppendLine($"**{T("warning", locale)}**");

            if (!canUseUpdateChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{updateChannel.Id}>"));
            if (!canUseReleaseChannel)
                body.AppendLine(T("error.missingMessagePerms", locale, $"<#{releaseChannel.Id}>"));
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("observer.title", locale))
            .WithDescription(body.ToString())
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
