// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Import;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Tasks;

public partial class TaskModule
{
    [SlashCommand("import", "Import tasks in jsonl format")]
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
            logger.LogTrace("Attempting to load task file...");
            fileContent = await httpClient.GetStringAsync(file.Url);
            logger.LogTrace("Successfully loaded task file");
        }
        catch (Exception e)
        {
            logger.LogError("Failed to get task file: {Exception}", e);
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

        var query = new ImportTaskCommand(projectId, fileContent, requestedBy);

        var result = await importHandler
            .HandleAsync(query)
            .ThenAsync(_ =>
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

        var addedCount = result.Value.Item1.AddedTaskCount;
        var pData = result.Value.Item2;

        var args = new Dictionary<string, object> { ["number"] = addedCount };

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithProjectInfo(pData, locale)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("task.import.success", locale, args));

        await interaction.FollowupAsync(embed: successEmbed.Build());
        return ExecutionResult.Success;
    }
}
