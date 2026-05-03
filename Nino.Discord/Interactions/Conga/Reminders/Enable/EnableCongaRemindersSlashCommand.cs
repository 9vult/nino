// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.CongaReminders.Enable;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Conga;

public partial class CongaModule
{
    public partial class RemindersModule
    {
        [SlashCommand("enable", "Enable Conga reminders for a project")]
        public async Task<RuntimeResult> EnableAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            [MinValue(1), MaxValue(90)] int days = 0
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

            var period = TimeSpan.FromDays(days);

            var command = new EnableCongaRemindersCommand(
                ProjectId: projectId,
                Period: period,
                RequestedBy: requestedBy
            );

            var result = await enableHandler
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
                .WithDescription(T("conga.reminders.enable.success", locale, period.TotalHours))
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
