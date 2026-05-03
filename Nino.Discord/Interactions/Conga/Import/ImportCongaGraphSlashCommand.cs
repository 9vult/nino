// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Conga.Import;
using Nino.Core.Features.Queries.Projects.Conga.GetDot;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Conga;

public partial class CongaModule
{
    [SlashCommand("import", "Import a new Conga graph")]
    public async Task<RuntimeResult> ImportAsync(
        [MaxLength(Length.Alias), Autocomplete(typeof(ProjectAutocompleteHandler))] Alias alias,
        IAttachment file
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolveProject = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (!resolveProject.IsSuccess)
        {
            return await interaction.FailAsync(
                resolveProject.Status,
                locale,
                new FailureContext { Alias = alias }
            );
        }

        var projectId = resolveProject.Value;

        string fileContent;
        try
        {
            logger.LogTrace("Attempting to load conga file...");
            fileContent = await httpClient.GetStringAsync(file.Url);
            logger.LogTrace("Successfully loaded conga file");
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get conga file: {Exception}", e);
            return await interaction.FailAsync(
                ResultStatus.BadRequest,
                locale,
                new FailureContext
                {
                    Alias = alias,
                    Overrides = new Dictionary<ResultStatus, string>
                    {
                        [ResultStatus.BadRequest] = "import.failed",
                    },
                }
            );
        }

        var query = new ImportCongaCommand(projectId, fileContent, requestedBy);

        var result = await importHandler
            .HandleAsync(query)
            .BindAsync(() =>
                getProjectDataHandler.HandleAsync(new GetGenericProjectDataQuery(projectId))
            )
            .ThenAsync(_ => getDotHandler.HandleAsync(new GetCongaDotQuery(projectId, null)));

        var pData = result.Value.Item1;
        var dot = result.Value.Item2;

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale, includePoster: false)
            .WithDescription(T("conga.import.success", locale));

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
                using var stream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
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
            successEmbed = successEmbed.WithDescription(
                $"Failed to generate Conga image: {response.StatusCode}: {response.ReasonPhrase}"
            );
        }
        else
        {
            successEmbed = successEmbed.WithDescription(T("conga.empty", locale));
        }

        await interaction.FollowupAsync(embed: successEmbed.Build());
        return ExecutionResult.Success;
    }
}
