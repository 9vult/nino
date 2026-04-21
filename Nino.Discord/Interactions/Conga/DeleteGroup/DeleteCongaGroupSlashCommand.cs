// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Conga.RemoveGroup;
using Nino.Core.Features.Queries.Projects.Conga.GetDot;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Discord.Handlers.AutocompleteHandlers.Conga;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Conga;

public partial class CongaModule
{
    public partial class GroupModule
    {
        [SlashCommand("delete", "Remove a group from a project's Conga graph")]
        public async Task<RuntimeResult> DeleteGroupAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            [MaxLength(Length.Abbreviation), Autocomplete(typeof(CongaGroupsAutocompleteHandler))]
                Abbreviation name
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

            var command = new RemoveCongaGroupCommand(
                ProjectId: projectId,
                RequestedBy: requestedBy,
                Name: name
            );

            var result = await deleteGroupHandler
                .HandleAsync(command)
                .BindAsync(() =>
                    getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
                )
                .ThenAsync(_ => getDotHandler.HandleAsync(new GetCongaDotQuery(projectId, null)));

            // Not using the helper because of the high complexity of the responses
            if (!result.IsSuccess)
            {
                var key = result.Status switch
                {
                    ResultStatus.Unauthorized => "error.permissions",
                    ResultStatus.ProjectNotFound => "project.notFound",
                    ResultStatus.NotFound => "conga.group.remove.notfound",
                    _ => "error.generic",
                };
                var args = new Dictionary<string, object> { ["alias"] = alias, ["name"] = name };

                var embed = new EmbedBuilder()
                    .WithTitle("Baka.")
                    .WithDescription(T(key, locale, args))
                    .WithColor(0xd797ff)
                    .Build();
                await interaction.FollowupAsync(embed: embed, ephemeral: false);
                return ExecutionResult.Failure;
            }

            var pData = result.Value.Item1;
            var dot = result.Value.Item2;

            // Success!
            var successEmbed = new EmbedBuilder()
                .WithProjectInfo(pData, locale, includePoster: false)
                .WithTitle(T("project.modification.title", locale));

            if (!string.IsNullOrEmpty(dot))
            {
                var response = await httpClient.PostAsync(
                    "https://quickchart.io/graphviz",
                    new StringContent(
                        JsonSerializer.Serialize(
                            new
                            {
                                graph = dot,
                                layout = "dot",
                                format = "png",
                            }
                        ),
                        Encoding.UTF8,
                        "application/json"
                    )
                );

                if (response.IsSuccessStatusCode)
                {
                    using var stream = new MemoryStream(
                        await response.Content.ReadAsByteArrayAsync()
                    );
                    successEmbed = successEmbed
                        .WithImageUrl("attachment://congo.png")
                        .WithDescription(T("conga.group.remove.success", locale, name));
                    await interaction.FollowupWithFileAsync(
                        stream,
                        "congo.png",
                        embed: successEmbed.Build()
                    );
                    return ExecutionResult.Success;
                }

                logger.LogError(
                    "Failed to generate Conga image: {StatusCode}: {ReasonPhrase}",
                    response.StatusCode,
                    response.ReasonPhrase
                );
            }

            successEmbed = successEmbed.WithDescription(
                T("conga.group.remove.success.empty", locale, name)
            );

            await interaction.FollowupAsync(embed: successEmbed.Build());
            return ExecutionResult.Success;
        }
    }
}
