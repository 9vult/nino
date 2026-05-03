// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Admins.Remove;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    public partial class AdminModule
    {
        [SlashCommand("remove", "Remove an admin from a project")]
        public async Task<RuntimeResult> RemoveAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            SocketUser user
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

            var userId = await identityService.GetOrCreateUserByDiscordIdAsync(
                user.Id,
                user.Username
            );

            var command = new RemoveProjectAdminCommand(
                ProjectId: projectId,
                UserId: userId,
                RequestedBy: requestedBy
            );

            var result = await removeAdminHandler
                .HandleAsync(command)
                .BindAsync(() =>
                    getGenericDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
                );

            if (!result.IsSuccess)
            {
                return await interaction.FailAsync(
                    result.Status,
                    locale,
                    new FailureContext
                    {
                        Overrides = new Dictionary<ResultStatus, string>
                        {
                            [ResultStatus.BadRequest] = "admin.remove.badRequest",
                        },
                    }
                );
            }

            var pData = result.Value;

            // Success!
            var staffMention = $"<@{user.Id}>";
            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("admin.remove.success", locale, staffMention))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
