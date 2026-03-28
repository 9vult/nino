// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Core.Features.Commands.TemplateStaff.Edit;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.TemplateStaff;

public class EditTemplateStaffHandlerTests : TestBase
{
    [Test]
    public async Task AllEpisodes_Selection_Edits_All_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.AllEpisodes,
            AssigneeId: seed.User3Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);

        // Staff entry was edited
        var staff = project.TemplateStaff.First(s => s.Id == seed.TemplateStaff1Id);
        await Assert.That(staff.AssigneeId).IsEqualTo(seed.User3Id);
        await Assert.That(staff.Name).IsEqualTo("Test");

        // All episodes were edited
        var episode1 = project.Episodes.First(e => e.Id == seed.Episode1Id);
        var episode2 = project.Episodes.First(e => e.Id == seed.Episode2Id);
        await Assert
            .That(episode1.Tasks)
            .Contains(t => t.AssigneeId == seed.User3Id && t.Name == "Test");
        await Assert
            .That(episode2.Tasks)
            .Contains(t => t.AssigneeId == seed.User3Id && t.Name == "Test");
    }

    [Test]
    public async Task IncompleteEpisodes_Selection_Edits_Incomplete_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            AssigneeId: seed.User3Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);
        var episode1 = project.Episodes.First(e => e.Id == seed.Episode1Id);
        var episode2 = project.Episodes.First(e => e.Id == seed.Episode2Id);

        // Episode 1 is already done
        await Assert.That(episode1.Tasks).DoesNotContain(t => t.Name == command.Name);

        // Episode 2 is incomplete
        await Assert.That(episode2.Tasks).Contains(t => t.Name == command.Name);

        // Staff was edited
        await Assert.That(project.TemplateStaff).Contains(s => s.Name == command.Name);
    }

    [Test]
    public async Task FutureEpisodes_Selection_Edits_No_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.FutureEpisodes,
            AssigneeId: seed.User3Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);

        // No episodes have task
        await Assert.That(project.Episodes).All(e => e.Tasks.All(t => t.Name != command.Name));

        // Project has template staff
        await Assert.That(project.TemplateStaff).Contains(s => s.Name == command.Name);
    }

    [Test]
    public async Task AllEpisodes_Selection_Does_Not_Edit_When_Conflict_Exists_In_Any_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.AllEpisodes,
            Abbreviation: Abbreviation.From("TLC")
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskConflict);
    }

    [Test]
    public async Task IncompleteEpisodes_Selection_Does_Not_Edit_When_Conflict_Exists_In_Incomplete_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            Abbreviation: Abbreviation.From("KFX")
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskConflict);
    }

    [Test]
    public async Task IncompleteEpisodes_Selection_Adds_When_Conflict_Exists_In_Completed_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            Abbreviation: Abbreviation.From("STL")
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task FutureEpisodes_Selection_Adds_When_Conflict_Exists_In_Any_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.FutureEpisodes,
            Abbreviation: Abbreviation.From("STL")
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Edits_Do_Not_Edit_Differing_Task_Values()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Change name of episode 2's task
        var e2Task = await db.Tasks.FirstAsync(t => t.Id == seed.Task1Id2);
        e2Task.Name = "ERIS";
        await db.SaveChangesAsync();

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User1Id,
            TemplateStaffApplicator.AllEpisodes,
            AssigneeId: seed.User3Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);

        // Staff entry was edited
        var staff = project.TemplateStaff.First(s => s.Id == seed.TemplateStaff1Id);
        await Assert.That(staff.AssigneeId).IsEqualTo(seed.User3Id);
        await Assert.That(staff.Name).IsEqualTo("Test");

        // All episodes were edited, but only episode 1's name
        var episode1 = project.Episodes.First(e => e.Id == seed.Episode1Id);
        var episode2 = project.Episodes.First(e => e.Id == seed.Episode2Id);

        await Assert.That(episode1.Tasks).Contains(t => t.AssigneeId == seed.User3Id);
        await Assert.That(episode1.Tasks).Contains(t => t.Name == "Test");

        await Assert.That(episode2.Tasks).Contains(t => t.AssigneeId == seed.User3Id);
        await Assert.That(episode2.Tasks).DoesNotContain(t => t.Name == "Test");
        await Assert.That(episode2.Tasks).Contains(t => t.Name == "ERIS");
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<EditTemplateStaffHandler>.Instance
        );

        var command = new EditTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            seed.User2Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            Abbreviation: Abbreviation.From("TLC")
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
