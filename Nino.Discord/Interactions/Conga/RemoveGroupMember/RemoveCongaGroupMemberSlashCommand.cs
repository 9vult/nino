// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Conga.RemoveGroupMember;
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
        [SlashCommand("remove-member", "Remove a member from a Conga group")]
        public async Task<RuntimeResult> RemoveMemberAsync(
            [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
            [MaxLength(Length.Abbreviation), Autocomplete(typeof(CongaGroupsAutocompleteHandler))]
                Abbreviation group,
            [MaxLength(Length.Abbreviation), Autocomplete(typeof(ProjectTaskAutocompleteHandler))]
                Abbreviation taskName
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

            var command = new RemoveCongaGroupMemberCommand(
                ProjectId: projectId,
                RequestedBy: requestedBy,
                GroupName: group,
                NodeName: taskName
            );

            var result = await removeMemberHandler
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
                    ResultStatus.NotFound => "conga.member.remove.notFound",
                    ResultStatus.MissingProjectChannel => "error.missingProjectChannel",
                    _ => "error.generic",
                };
                var args = new Dictionary<string, object>
                {
                    ["alias"] = alias,
                    ["name"] = taskName,
                    ["group"] = group,
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
                .WithProjectInfo(pData, locale, includePoster: false)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("conga.member.remove.success", locale, taskName, group));

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
