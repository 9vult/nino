// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Conga.AddEdge;
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
    public partial class EdgeModule
    {
        [SlashCommand("add", "Add an edge to a project's Conga graph")]
        public async Task<RuntimeResult> AddEdgeAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            [MaxLength(Length.Abbreviation), Autocomplete(typeof(CongaFromAutocompleteHandler))]
                Abbreviation from,
            [MaxLength(Length.Abbreviation), Autocomplete(typeof(CongaToAutocompleteHandler))]
                Abbreviation to
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

            var command = new AddCongaEdgeCommand(
                ProjectId: projectId,
                RequestedBy: requestedBy,
                From: from,
                To: to
            );

            var result = await addEdgeHandler
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
                    ResultStatus.TaskNotFound => "task.resolutionFailed",
                    ResultStatus.BadRequest => $"conga.{result.Message}",
                    ResultStatus.CongaConflict => "",
                    _ => "error.generic",
                };
                var args = new Dictionary<string, object>
                {
                    ["alias"] = alias,
                    ["abbreviation"] = result.Message == "from" ? from : to,
                };

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
                .WithProjectInfo(pData, locale)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("conga.edge.add.success", locale, from, to));

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
                    successEmbed = successEmbed.WithImageUrl("attachment://congo.png");
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

            await interaction.FollowupAsync(embed: successEmbed.Build());
            return ExecutionResult.Success;
        }
    }
}
