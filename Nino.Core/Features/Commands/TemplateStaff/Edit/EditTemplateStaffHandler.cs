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
        if (command.NewAbbreviation is not null)
        {
            if (project.TemplateStaff.Any(s => s.Abbreviation == command.NewAbbreviation.Value))
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
                            .Any(t => t.Abbreviation == command.NewAbbreviation.Value)
                    )
                        return Fail(ResultStatus.TaskConflict);
                    break;
                case TemplateStaffApplicator.AllEpisodes:
                    if (
                        project
                            .Episodes.SelectMany(e => e.Tasks)
                            .Any(t => t.Abbreviation == command.NewAbbreviation.Value)
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
            foreach (var task in tasks.Where(task => task.AssigneeId == staff.AssigneeId))
                task.AssigneeId = command.AssigneeId.Value;
            staff.AssigneeId = command.AssigneeId.Value;
        }

        if (command.NewAbbreviation is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Abbreviation to {Abbreviation}.",
                staff.Id,
                command.NewAbbreviation.Value
            );
            foreach (var task in tasks.Where(task => task.Abbreviation == staff.Abbreviation))
                task.Abbreviation = command.NewAbbreviation.Value;
            staff.Abbreviation = command.NewAbbreviation.Value;
        }

        if (command.Name is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Name to {Name}.",
                staff.Id,
                command.Name
            );
            foreach (var task in tasks.Where(task => task.Name == staff.Name))
                task.Name = command.Name;
            staff.Name = command.Name;
        }

        if (command.Weight is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s Weight to {Weight}.",
                staff.Id,
                command.Weight.Value
            );
            foreach (var task in tasks.Where(task => task.Weight == staff.Weight))
                task.Weight = command.Weight.Value;
            staff.Weight = command.Weight.Value;
        }

        if (command.IsPseudo is not null)
        {
            logger.LogInformation(
                "Setting Template Staff {StaffId}'s IsPseudo to {IsPseudo}.",
                staff.Id,
                command.IsPseudo.Value
            );
            foreach (var task in tasks.Where(task => task.IsPseudo == staff.IsPseudo))
                task.IsPseudo = command.IsPseudo.Value;
            staff.IsPseudo = command.IsPseudo.Value;
        }

        await db.SaveChangesAsync();
        return Success();
    }
}
