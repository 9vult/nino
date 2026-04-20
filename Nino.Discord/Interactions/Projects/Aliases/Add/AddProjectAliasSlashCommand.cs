// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Aliases.Add;
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
        [SlashCommand("add", "Add an alias to a project")]
        public async Task<RuntimeResult> AddAsync(
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

            var command = new AddAliasCommand(
                ProjectId: projectId,
                Alias: alias,
                RequestedBy: requestedBy
            );

            var result = await addAliasHandler
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
                            [ResultStatus.BadRequest] = "project.alias.add.badRequest",
                        },
                    }
                );
            }

            var pData = result.Value;

            // Success!
            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("project.alias.add.success", locale, alias))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
