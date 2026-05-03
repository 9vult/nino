// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Aliases.Remove;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Projects;

public partial class ProjectModule
{
    public partial class AliasModule
    {
        [SlashCommand("remove", "Remove an alias from a project")]
        public async Task<RuntimeResult> RemoveAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))]
                Alias project,
            [MaxLength(Length.Alias)] Alias alias
        )
        {
            var interaction = Context.Interaction;
            var locale = interaction.UserLocale;

            var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(
                interaction
            );

            var resolve = await projectResolver.HandleAsync(
                new ResolveProjectQuery(project, groupId, requestedBy)
            );

            if (!resolve.IsSuccess)
            {
                return await interaction.FailAsync(
                    resolve.Status,
                    locale,
                    new FailureContext { Alias = project }
                );
            }

            var projectId = resolve.Value;

            var command = new RemoveAliasCommand(
                ProjectId: projectId,
                Alias: alias,
                RequestedBy: requestedBy
            );

            var result = await removeAliasHandler
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
                            [ResultStatus.BadRequest] = "project.alias.remove.badRequest",
                        },
                    }
                );
            }

            var pData = result.Value;

            // Success!
            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("project.alias.remove.success", locale, alias))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
