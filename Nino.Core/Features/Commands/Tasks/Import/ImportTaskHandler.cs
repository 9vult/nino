// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Nino.Core.Dtos;
using Nino.Core.Features.Commands.Tasks.Add;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.Tasks.Import.ImportTaskResponse>;

namespace Nino.Core.Features.Commands.Tasks.Import;

public sealed class ImportTaskHandler(
    NinoDbContext db,
    IIdentityService identityService,
    IUserVerificationService verificationService,
    ILogger<AddTaskHandler> logger
) : ICommandHandler<ImportTaskCommand, Result<ImportTaskResponse>>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <inheritdoc />
    public async Task<Result<ImportTaskResponse>> HandleAsync(ImportTaskCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var allEpisodes = (
            await db.Episodes.Where(e => e.ProjectId == command.ProjectId).ToListAsync()
        )
            .OrderBy(e => e.Number.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var added = 0;
        var lines = command.Data.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            var input = JsonSerializer.Deserialize<TaskImportDto>(line, JsonSerializerOptions);
            if (input is null)
            {
                logger.LogWarning("Failed to deserialize task import \"{Input}\"", line);
                continue;
            }

            UserId assigneeId;
            // Assignee lookup
            if (input.Assignee.Id is not null)
                assigneeId = UserId.From(input.Assignee.Id.Value);
            else if (input.Assignee.DiscordId is not null)
                assigneeId = await identityService.GetOrCreateUserByDiscordIdAsync(
                    input.Assignee.DiscordId.Value
                );
            else
                continue;

            var firstIdx = allEpisodes.FindIndex(e => e.Number == input.First);
            var lastIdx = allEpisodes.FindIndex(e => e.Number == (input.Last ?? input.First)) + 1;

            if (firstIdx < 0)
            {
                logger.LogWarning(
                    "Failed to find episode {EpisodeNumber} for project {ProjectId}",
                    input.First,
                    command.ProjectId
                );
                continue;
            }

            if (lastIdx < 1)
            {
                logger.LogWarning(
                    "Failed to find episode {EpisodeNumber} for project {ProjectId}",
                    input.Last,
                    command.ProjectId
                );
                continue;
            }

            var episodes = allEpisodes[firstIdx..lastIdx].ToList();

            // Check for conflicts
            if (episodes.Any(e => e.Tasks.Any(t => t.Abbreviation == input.Abbreviation)))
                continue;

            // Add to episodes
            foreach (var episode in episodes)
            {
                episode.Tasks.Add(
                    new Nino.Domain.Entities.Task
                    {
                        ProjectId = command.ProjectId,
                        EpisodeId = episode.Id,
                        AssigneeId = assigneeId,
                        Abbreviation = input.Abbreviation,
                        Name = input.Name,
                        Weight =
                            input.Weight
                            ?? episode.Tasks.Select(s => s.Weight).DefaultIfEmpty(0).Max() + 1,
                        IsPseudo = input.IsPseudo,
                        IsDone = false,
                    }
                );
                episode.IsDone = false;
                episode.UpdatedAt = DateTime.UtcNow;
            }

            logger.LogInformation(
                "Adding Task {Abbreviation} to project {ProjectId} and applying to {EpisodeCount} episodes",
                input.Abbreviation,
                command.ProjectId,
                episodes.Count
            );
            added++;
        }

        await db.SaveChangesAsync();
        return Success(new ImportTaskResponse(added));
    }
}
