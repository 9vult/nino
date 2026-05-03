// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.AirNotifications.Enable;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    public partial class AirNotificationsModule
    {
        [SlashCommand("enable", "Enable air notifications for a project")]
        public async Task<RuntimeResult> EnableAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            SocketUser? user = null,
            SocketRole? role = null,
            [MinValue(-24), MaxValue(24)] int delayHours = 0,
            [MinValue(-60), MaxValue(60)] int delayMinutes = 0
        )
        {
            var interaction = Context.Interaction;
            var locale = interaction.UserLocale;

            var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(
                interaction
            );

            var resolve = await projectResolver.HandleAsync(
                new ResolveProjectQuery(alias, groupId, requestedBy)
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

            var delay = TimeSpan.FromHours(delayHours).Add(TimeSpan.FromMinutes(delayMinutes));

            var userId = user is null
                ? null
                : (UserId?)
                    await identityService.GetOrCreateUserByDiscordIdAsync(user.Id, user.Username);

            var roleId = role is null
                ? null
                : (RoleId?)await identityService.GetOrCreateRoleByDiscordIdAsync(role.Id);

            var command = new EnableAirNotificationsCommand(
                ProjectId: projectId,
                NotificationUserId: userId,
                NotificationRoleId: roleId,
                Delay: delay,
                RequestedBy: requestedBy
            );

            var result = await enableHandler
                .HandleAsync(command)
                .BindAsync(() =>
                    getGenericDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
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

            // Success!
            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("project.airNotifications.enable.success", locale))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
