// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.DelegateObserver.Remove;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Observers;

public partial class ObserverModule
{
    public partial class DelegateModule
    {
        [SlashCommand("remove", "Remove a delegate observer")]
        public async Task<RuntimeResult> RemoveAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias
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

            var command = new RemoveDelegateObserverCommand(projectId, requestedBy);

            var result = await removeHandler
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

            // Success!
            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("observer.delegate.remove.success", locale))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
