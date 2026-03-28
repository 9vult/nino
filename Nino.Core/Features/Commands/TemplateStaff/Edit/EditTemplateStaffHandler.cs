// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.TemplateStaff.Edit;

public sealed class EditTemplateStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EditTemplateStaffHandler> logger
) : ICommandHandler<EditTemplateStaffCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(EditTemplateStaffCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var staff = project.TemplateStaff.FirstOrDefault(t => t.Id == command.TemplateStaffId);
        if (staff is null)
            return Fail(ResultStatus.TemplateStaffNotFound);

        // Check for conflicts, if applicable
        if (command.Abbreviation is not null)
        {
            if (project.TemplateStaff.Any(s => s.Abbreviation == command.Abbreviation.Value))
                return Fail(ResultStatus.TaskConflict);

            switch (command.Applicator)
            {
                case TemplateStaffApplicator.FutureEpisodes:
                    break;
                case TemplateStaffApplicator.IncompleteEpisodes:
                    if (
                        project
                            .Episodes.Where(e => !e.IsDone)
                            .SelectMany(e => e.Tasks)
                            .Any(t => t.Abbreviation == command.Abbreviation.Value)
                    )
                        return Fail(ResultStatus.TaskConflict);
                    break;
                case TemplateStaffApplicator.AllEpisodes:
                    if (
                        project
                            .Episodes.SelectMany(e => e.Tasks)
                            .Any(t => t.Abbreviation == command.Abbreviation.Value)
                    )
                        return Fail(ResultStatus.TaskConflict);
                    break;
            }
        }

        var tasks = (
            command.Applicator switch
            {
                TemplateStaffApplicator.AllEpisodes => project.Episodes,
                TemplateStaffApplicator.IncompleteEpisodes => project
                    .Episodes.Where(e => !e.IsDone)
                    .ToList(),
                _ => [],
            }
        )
            .SelectMany(e => e.Tasks.Where(t => t.Abbreviation == staff.Abbreviation))
            .ToList();

        if (command.AssigneeId is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Assignee to {AssigneeId}.",
                staff.Id,
                command.AssigneeId
            );
            staff.AssigneeId = command.AssigneeId.Value;
            foreach (var task in tasks)
                task.AssigneeId = command.AssigneeId.Value;
        }

        if (command.Abbreviation is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Abbreviation to {Abbreviation}.",
                staff.Id,
                command.Abbreviation.Value
            );
            staff.Abbreviation = command.Abbreviation.Value;
            foreach (var task in tasks)
                task.Abbreviation = command.Abbreviation.Value;
        }

        if (command.Name is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Name to {Name}.",
                staff.Id,
                command.Name
            );
            staff.Name = command.Name;
            foreach (var task in tasks)
                task.Name = command.Name;
        }

        if (command.Weight is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Weight to {Weight}.",
                staff.Id,
                command.Weight.Value
            );
            staff.Weight = command.Weight.Value;
            foreach (var task in tasks)
                task.Weight = command.Weight.Value;
        }

        if (command.IsPseudo is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s IsPseudo to {IsPseudo}.",
                staff.Id,
                command.IsPseudo.Value
            );
            staff.IsPseudo = command.IsPseudo.Value;
            foreach (var task in tasks)
                task.IsPseudo = command.IsPseudo.Value;
        }

        await db.SaveChangesAsync();
        return Success();
    }
}
