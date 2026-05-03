// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Add;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Tasks;

public class AddTaskHandlerTests : TestBase
{
    [Test]
    public async Task Single_Episode_Selection_Adds_To_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTaskHandler(
            db,
            UserVerificationService,
            NullLogger<AddTaskHandler>.Instance
        );

        var command = new AddTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            seed.User1Id,
            seed.User1Id,
            Abbreviation.From("TL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var episode1 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode1Id);
        await Assert.That(episode1.IsDone).IsFalse(); // Was done
        await Assert.That(episode1.Tasks).Contains(t => t.Abbreviation == command.Abbreviation);
    }

    [Test]
    public async Task Single_Episode_Selection_With_Conflict_Does_Not_Add_To_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTaskHandler(
            db,
            UserVerificationService,
            NullLogger<AddTaskHandler>.Instance
        );

        var command = new AddTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            seed.User1Id,
            seed.User1Id,
            Abbreviation.From("ED"),
            "Editing",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskConflict);
    }

    [Test]
    public async Task Multiple_Episode_Selection_Adds_To_Episodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTaskHandler(
            db,
            UserVerificationService,
            NullLogger<AddTaskHandler>.Instance
        );

        var command = new AddTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            seed.User1Id,
            seed.User1Id,
            Abbreviation.From("TL"),
            "Translation",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var episode1 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode1Id);
        var episode2 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode2Id);

        await Assert.That(episode1.IsDone).IsFalse();
        await Assert.That(episode2.IsDone).IsFalse();
        await Assert.That(episode1.Tasks).Contains(t => t.Abbreviation == command.Abbreviation);
        await Assert.That(episode2.Tasks).Contains(t => t.Abbreviation == command.Abbreviation);
    }

    [Test]
    public async Task Multiple_Episode_Selection_With_Conflict_Does_Not_Add_To_Episodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTaskHandler(
            db,
            UserVerificationService,
            NullLogger<AddTaskHandler>.Instance
        );

        var command = new AddTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            seed.User1Id,
            seed.User1Id,
            Abbreviation.From("KFX"),
            "Song Styling",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskConflict);
    }

    [Test]
    public async Task Invalid_EpisodeNumber_ReturnsFailure()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTaskHandler(
            db,
            UserVerificationService,
            NullLogger<AddTaskHandler>.Instance
        );

        var command = new AddTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            EpisodeId.FromNewGuid(),
            seed.User1Id,
            seed.User1Id,
            Abbreviation.From("KFX"),
            "Song Styling",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.EpisodeNotFound);
        await Assert.That(result.Message).IsEqualTo("last");
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddTaskHandler(
            db,
            UserVerificationService,
            NullLogger<AddTaskHandler>.Instance
        );

        var command = new AddTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            EpisodeId.FromNewGuid(),
            seed.User2Id,
            seed.User1Id,
            Abbreviation.From("KFX"),
            "Song Styling",
            false
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
