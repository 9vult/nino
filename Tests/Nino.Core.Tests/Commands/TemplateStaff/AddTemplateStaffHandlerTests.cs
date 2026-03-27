// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Core.Features.Commands.TemplateStaff.Add;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.TemplateStaff;

public class AddTemplateStaffTests : TestBase
{
    [Test]
    public async Task AllEpisodes_Selection_Adds_To_All_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.AllEpisodes,
            seed.User1Id,
            Abbreviation.From("TL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);

        // All episodes are incomplete
        await Assert.That(project.Episodes).All(e => !e.IsDone);

        // All episodes have task
        await Assert
            .That(project.Episodes)
            .All(e => e.Tasks.Any(t => t.Abbreviation == command.Abbreviation));

        // Project has template staff
        await Assert
            .That(project.TemplateStaff)
            .Contains(s => s.Abbreviation == command.Abbreviation);
    }

    [Test]
    public async Task IncompleteEpisodes_Selection_Adds_To_Incomplete_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            seed.User1Id,
            Abbreviation.From("TL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);
        var episode1 = project.Episodes.First(e => e.Id == seed.Episode1Id);
        var episode2 = project.Episodes.First(e => e.Id == seed.Episode2Id);

        // Episode 1 is already done
        await Assert.That(episode1.IsDone).IsTrue();
        await Assert
            .That(episode1.Tasks)
            .DoesNotContain(t => t.Abbreviation == command.Abbreviation);

        // Episode 2 is incomplete
        await Assert.That(episode2.IsDone).IsFalse();
        await Assert.That(episode2.Tasks).Contains(t => t.Abbreviation == command.Abbreviation);

        // Project has template staff
        await Assert
            .That(project.TemplateStaff)
            .Contains(s => s.Abbreviation == command.Abbreviation);
    }

    [Test]
    public async Task FutureEpisodes_Selection_Adds_To_No_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.FutureEpisodes,
            seed.User1Id,
            Abbreviation.From("TL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);

        var episode1 = project.Episodes.First(e => e.Id == seed.Episode1Id);
        var episode2 = project.Episodes.First(e => e.Id == seed.Episode2Id);

        // No change to completion
        await Assert.That(episode1.IsDone).IsTrue();
        await Assert.That(episode2.IsDone).IsFalse();

        // No episodes have task
        await Assert
            .That(project.Episodes)
            .All(e => e.Tasks.All(t => t.Abbreviation != command.Abbreviation));

        // Project has template staff
        await Assert
            .That(project.TemplateStaff)
            .Contains(s => s.Abbreviation == command.Abbreviation);
    }

    [Test]
    public async Task AllEpisodes_Selection_Does_Not_Add_When_Conflict_Exists_In_Any_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.AllEpisodes,
            seed.User1Id,
            Abbreviation.From("TLC"),
            "Translation Checking",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskConflict);
    }

    [Test]
    public async Task IncompleteEpisodes_Selection_Does_Not_Add_When_Conflict_Exists_In_Incomplete_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            seed.User1Id,
            Abbreviation.From("KFX"),
            "Karaoke",
            false
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

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            seed.User1Id,
            Abbreviation.From("STL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task FutureEpisodes_Selection_Adds_When_Conflict_Exists_In_Any_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User1Id,
            TemplateStaffApplicator.FutureEpisodes,
            seed.User1Id,
            Abbreviation.From("STL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<AddTemplateStaffHandler>.Instance
        );

        var command = new AddTemplateStaffCommand(
            seed.ProjectId,
            seed.User2Id,
            TemplateStaffApplicator.FutureEpisodes,
            seed.User1Id,
            Abbreviation.From("STL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
