// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Remove;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Tasks;

public class RemoveTaskHandlerTests : TestBase
{
    [Test]
    public async Task Single_Episode_Selection_Removes_From_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var abbreviation = await db
            .TemplateStaff.Where(s => s.Id == seed.TemplateStaff1Id)
            .Select(s => s.Abbreviation)
            .FirstAsync();

        var handler = new RemoveTaskHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTaskHandler>.Instance
        );

        var command = new RemoveTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            Abbreviation.From("ED"),
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");
        await Assert.That(result.Value!.CompletedEpisodes.Count).IsEqualTo(1); // episode 1

        var episode1 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode1Id);
        var episode2 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode2Id);

        await Assert.That(episode1.IsDone).IsTrue();
        await Assert.That(episode1.Tasks).DoesNotContain(t => t.Abbreviation == abbreviation);
        await Assert.That(episode2.Tasks).Contains(t => t.Abbreviation == abbreviation);
    }

    [Test]
    public async Task Multiple_Episode_Selection_Removes_From_Episodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var abbreviation = await db
            .TemplateStaff.Where(s => s.Id == seed.TemplateStaff1Id)
            .Select(s => s.Abbreviation)
            .FirstAsync();

        var handler = new RemoveTaskHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTaskHandler>.Instance
        );

        var command = new RemoveTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");
        await Assert.That(result.Value!.CompletedEpisodes.Count).IsEqualTo(1); // episode 1

        var episode1 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode1Id);
        var episode2 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode2Id);

        await Assert.That(episode1.Tasks).DoesNotContain(t => t.Abbreviation == abbreviation);
        await Assert.That(episode2.Tasks).DoesNotContain(t => t.Abbreviation == abbreviation);
    }

    [Test]
    public async Task Invalid_EpisodeNumber_ReturnsFailure()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveTaskHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTaskHandler>.Instance
        );

        var command = new RemoveTaskCommand(
            seed.ProjectId,
            EpisodeId.FromNewGuid(),
            seed.Episode1Id,
            Abbreviation.From("ED"),
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.EpisodeNotFound);
        await Assert.That(result.Message).IsEqualTo("first");
    }

    [Test]
    public async Task Invalid_Task_ReturnsFailure()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveTaskHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTaskHandler>.Instance
        );

        var command = new RemoveTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            Abbreviation.From("ERIS"),
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskNotFound);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveTaskHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveTaskHandler>.Instance
        );

        var command = new RemoveTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            seed.User2Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
