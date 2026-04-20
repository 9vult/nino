// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Conga.RemoveEdge;
using Nino.Core.Features.Queries.Projects.Conga.GetDot;
using Nino.Core.Features.Queries.Projects.Conga.ListEdges;
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
        [SlashCommand("remove", "Remove an edge to a project's Conga graph")]
        public async Task<RuntimeResult> RemoveEdgeAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            [Autocomplete(typeof(CongaEdgesAutocompleteHandler))] string edge
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

            var deserialized = JsonSerializer.Deserialize<ListCongaEdgesResult>(edge);

            if (deserialized is null)
                return await interaction.FailAsync(
                    ResultStatus.BadRequest,
                    locale,
                    new FailureContext
                    {
                        Alias = alias,
                        Overrides = new Dictionary<ResultStatus, string>
                        {
                            [ResultStatus.BadRequest] = "conga.noLink",
                        },
                    }
                );

            var (from, to) = deserialized;

            var command = new RemoveCongaEdgeCommand(
                ProjectId: projectId,
                RequestedBy: requestedBy,
                From: from,
                To: to
            );

            var result = await removeEdgeHandler
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
                    ResultStatus.BadRequest => $"conga.{result.Message}",
                    ResultStatus.CongaConflict => "conga.edge.add.conflict",
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
                        .WithDescription(T("conga.edge.remove.success", locale, from, to));

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
                T("conga.edge.remove.success.empty", locale, from, to)
            );

            await interaction.FollowupAsync(embed: successEmbed.Build());
            return ExecutionResult.Success;
        }
    }
}
