// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.Edit;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Tasks;

public class EditTaskHandlerTests : TestBase
{
    [Test]
    public async Task Single_Episode_Selection_Edits_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            Abbreviation.From("ED"),
            seed.User1Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var episode1 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode1Id);
        await Assert.That(episode1.Tasks).Contains(t => t.Name == command.Name);
    }

    [Test]
    public async Task Single_Episode_Selection_With_Conflict_Does_Not_Edit_Episode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            Abbreviation.From("TLC"),
            seed.User1Id,
            NewAbbreviation: Abbreviation.From("ED")
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.TaskConflict);
    }

    [Test]
    public async Task Multiple_Episode_Selection_Edits_Episodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            seed.User1Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var episode1 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode1Id);
        var episode2 = await db.Episodes.FirstAsync(e => e.Id == seed.Episode2Id);

        await Assert.That(episode1.Tasks).Contains(t => t.Name == command.Name);
        await Assert.That(episode2.Tasks).Contains(t => t.Name == command.Name);
    }

    [Test]
    public async Task Multiple_Episode_Selection_With_Conflict_Does_Not_Edit_Episodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            seed.User1Id,
            NewAbbreviation: Abbreviation.From("KFX")
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

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            EpisodeId.FromNewGuid(),
            seed.Episode1Id,
            Abbreviation.From("ED"),
            seed.User1Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.EpisodeNotFound);
        await Assert.That(result.Message).IsEqualTo("first");
    }

    [Test]
    public async Task Invalid_TaskId_ReturnsFailure()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            Abbreviation.From("ERIS"),
            seed.User1Id,
            NewAbbreviation: Abbreviation.From("ED")
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

        var handler = new EditTaskHandler(
            db,
            UserVerificationService,
            NullLogger<EditTaskHandler>.Instance
        );

        var command = new EditTaskCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            Abbreviation.From("ED"),
            seed.User2Id,
            Name: "Test"
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
