// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.TemplateStaff;
using Nino.Core.Features.Commands.TemplateStaff.Remove;

namespace Nino.Core.Tests.Commands.TemplateStaff;

public class RemoveTemplateStaffHandlerTests : TestBase
{
    [Test]
    public async Task AllEpisodes_Selection_Removes_From_All_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var abbreviation = await db
            .TemplateStaff.Where(s => s.Id == seed.TemplateStaff1Id)
            .Select(s => s.Abbreviation)
            .FirstAsync();

        var handler = new RemoveTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTemplateStaffHandler>.Instance
        );

        var command = new RemoveTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            TemplateStaffApplicator.AllEpisodes,
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");
        await Assert.That(result.Value!.CompletedEpisodes.Count).IsEqualTo(1); // episode 1

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstAsync(p => p.Id == command.ProjectId);
        var episode1 = project.Episodes.First(e => e.Id == seed.Episode1Id);
        var episode2 = project.Episodes.First(e => e.Id == seed.Episode2Id);

        // Episode 1 is already done
        await Assert.That(episode1.IsDone).IsTrue();
        await Assert.That(episode1.Tasks).DoesNotContain(t => t.Abbreviation == abbreviation);

        // Episode 2 is incomplete
        await Assert.That(episode2.IsDone).IsFalse();
        await Assert.That(episode2.Tasks).DoesNotContain(t => t.Abbreviation == abbreviation);

        // Project does not have template staff
        await Assert
            .That(project.TemplateStaff)
            .DoesNotContain(t => t.Abbreviation == abbreviation);
    }

    [Test]
    public async Task IncompleteEpisodes_Selection_Removes_From_Incomplete_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var abbreviation = await db
            .TemplateStaff.Where(s => s.Id == seed.TemplateStaff1Id)
            .Select(s => s.Abbreviation)
            .FirstAsync();

        var handler = new RemoveTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTemplateStaffHandler>.Instance
        );

        var command = new RemoveTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            seed.User1Id
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
        await Assert.That(episode1.Tasks).Contains(t => t.Abbreviation == abbreviation);

        // Episode 2 is incomplete
        await Assert.That(episode2.IsDone).IsFalse();
        await Assert.That(episode2.Tasks).DoesNotContain(t => t.Abbreviation == abbreviation);

        // Project does not have template staff
        await Assert
            .That(project.TemplateStaff)
            .DoesNotContain(t => t.Abbreviation == abbreviation);
    }

    [Test]
    public async Task FutureEpisodes_Selection_Removes_From_No_Episodes_And_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var abbreviation = await db
            .TemplateStaff.Where(s => s.Id == seed.TemplateStaff1Id)
            .Select(s => s.Abbreviation)
            .FirstAsync();

        var handler = new RemoveTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTemplateStaffHandler>.Instance
        );

        var command = new RemoveTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            TemplateStaffApplicator.FutureEpisodes,
            seed.User1Id
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
        await Assert.That(episode1.Tasks).Contains(t => t.Abbreviation == abbreviation);

        // Episode 2 is incomplete
        await Assert.That(episode2.IsDone).IsFalse();
        await Assert.That(episode2.Tasks).Contains(t => t.Abbreviation == abbreviation);

        // Project does not have template staff
        await Assert
            .That(project.TemplateStaff)
            .DoesNotContain(t => t.Abbreviation == abbreviation);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveTemplateStaffHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTemplateStaffHandler>.Instance
        );

        var command = new RemoveTemplateStaffCommand(
            seed.ProjectId,
            seed.TemplateStaff1Id,
            TemplateStaffApplicator.IncompleteEpisodes,
            seed.User2Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
