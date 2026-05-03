// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Episodes.Add;
using Nino.Core.Features.Commands.Episodes.Remove;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Episodes;

public class RemoveEpisodeHandlerTests : TestBase
{
    [Test]
    public async Task SingleEpisode_RemovesEpisode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup
        await new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        ).HandleAsync(new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("3"), 3));

        var project = await db
            .Projects.Include(p => p.Episodes)
            .Where(p => p.Id == seed.ProjectId)
            .FirstAsync();

        var episode3Id = project.Episodes.First(e => e.Number == "3").Id;

        var handler = new RemoveEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveEpisodeHandler>.Instance
        );

        var command = new RemoveEpisodeCommand(
            seed.ProjectId,
            episode3Id,
            episode3Id,
            seed.User1Id
        );
        var result = await handler.HandleAsync(command);
        db.ChangeTracker.Clear();

        await Assert.That(result.IsSuccess).IsTrue();

        var episodes = await db.Episodes.Where(e => e.ProjectId == seed.ProjectId).ToListAsync();

        await Assert.That(episodes).Contains(e => e.Number == Number.From("1"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("2"));
        await Assert.That(episodes).DoesNotContain(e => e.Number == Number.From("3"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("4"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("5"));
    }

    [Test]
    public async Task MultipleEpisodes_RemovesEpisodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup
        await new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        ).HandleAsync(new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("3"), 3));

        var project = await db
            .Projects.Include(p => p.Episodes)
            .Where(p => p.Id == seed.ProjectId)
            .FirstAsync();

        var episode2Id = project.Episodes.First(e => e.Number == "2").Id;
        var episode4Id = project.Episodes.First(e => e.Number == "4").Id;

        var handler = new RemoveEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveEpisodeHandler>.Instance
        );

        var command = new RemoveEpisodeCommand(
            seed.ProjectId,
            episode2Id,
            episode4Id,
            seed.User1Id
        );
        var result = await handler.HandleAsync(command);
        db.ChangeTracker.Clear();

        await Assert.That(result.IsSuccess).IsTrue();

        var episodes = await db.Episodes.Where(e => e.ProjectId == seed.ProjectId).ToListAsync();

        await Assert.That(episodes).Contains(e => e.Number == Number.From("1"));
        await Assert.That(episodes).DoesNotContain(e => e.Number == Number.From("2"));
        await Assert.That(episodes).DoesNotContain(e => e.Number == Number.From("3"));
        await Assert.That(episodes).DoesNotContain(e => e.Number == Number.From("4"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("5"));
    }

    [Test]
    public async Task InvalidEpisode_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveEpisodeHandler>.Instance
        );

        var command = new RemoveEpisodeCommand(
            seed.ProjectId,
            EpisodeId.FromNewGuid(),
            seed.Episode1Id,
            seed.User1Id
        );
        var result = await handler.HandleAsync(command);
        db.ChangeTracker.Clear();

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.EpisodeNotFound);
        await Assert.That(result.Message).IsEqualTo("first");
    }

    [Test]
    public async Task NoPermissions_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveEpisodeHandler>.Instance
        );

        var command = new RemoveEpisodeCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode1Id,
            seed.User2Id
        );
        var result = await handler.HandleAsync(command);
        db.ChangeTracker.Clear();

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
