// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Episodes.Add;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Episodes;

public class AddEpisodeHandlerTests : TestBase
{
    [Test]
    public async Task SingleEpisode_With_Number_AddsEpisode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("3"));
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var episode = await db
            .Episodes.Where(e => e.ProjectId == seed.ProjectId && e.Number == Number.From("3"))
            .FirstOrDefaultAsync();

        await Assert.That(episode).IsNotNull();
        await Assert.That(episode.IsDone).IsFalse();
        await Assert.That(episode.Tasks).IsNotEmpty();
    }

    [Test]
    public async Task MultipleEpisodes_With_Number_AddsEpisodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("3"), 3);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var episodes = await db.Episodes.Where(e => e.ProjectId == seed.ProjectId).ToListAsync();

        await Assert.That(episodes).Contains(e => e.Number == Number.From("3"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("4"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("5"));
        await Assert.That(episodes).DoesNotContain(e => e.Number == Number.From("6"));
    }

    [Test]
    public async Task SingleEpisode_With_NonNumber_AddsEpisode()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("OVA 1"));
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var episode = await db
            .Episodes.Where(e => e.ProjectId == seed.ProjectId && e.Number == Number.From("OVA 1"))
            .FirstOrDefaultAsync();

        await Assert.That(episode).IsNotNull();
        await Assert.That(episode.IsDone).IsFalse();
        await Assert.That(episode.Tasks).IsNotEmpty();
    }

    [Test]
    public async Task MultipleEpisodes_With_NonNumber_And_Format_AddsEpisodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(
            seed.ProjectId,
            seed.User1Id,
            Number.From("3"),
            3,
            "OVA $"
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var episodes = await db.Episodes.Where(e => e.ProjectId == seed.ProjectId).ToListAsync();

        await Assert.That(episodes).Contains(e => e.Number == Number.From("OVA 3"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("OVA 4"));
        await Assert.That(episodes).Contains(e => e.Number == Number.From("OVA 5"));
        await Assert.That(episodes).DoesNotContain(e => e.Number == Number.From("OVA 6"));
    }

    [Test]
    public async Task Decimal_Comma_Becomes_Period()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("3,5"));
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var episode = await db
            .Episodes.Where(e => e.ProjectId == seed.ProjectId && e.Number == Number.From("3.5"))
            .FirstOrDefaultAsync();

        await Assert.That(episode).IsNotNull();
    }

    [Test]
    public async Task NonDecimal_Comma_Does_Not_Become_Period()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("3,5,7"));
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var episode = await db
            .Episodes.Where(e => e.ProjectId == seed.ProjectId && e.Number == Number.From("3,5,7"))
            .FirstOrDefaultAsync();

        await Assert.That(episode).IsNotNull();
    }

    [Test]
    public async Task InvalidFormat_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(
            seed.ProjectId,
            seed.User1Id,
            Number.From("3"),
            1,
            "asdf"
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
        await Assert.That(result.Message).IsEqualTo("bad-format");
    }

    [Test]
    public async Task NotUsingFormatForNonNumber_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("OVA 1"), 3);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
        await Assert.That(result.Message).IsEqualTo("not-number");
    }

    [Test]
    public async Task FormattedNumberIsTooLong_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(
            seed.ProjectId,
            seed.User1Id,
            Number.From("3"),
            1,
            "I broke up with my ex girl. Here's her number: $"
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
        await Assert.That(result.Message).IsEqualTo("too-long");
    }

    [Test]
    public async Task SingleDuplicateEpisode_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("1"));
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.EpisodeConflict);
    }

    [Test]
    public async Task PartialDuplicateEpisode_AddsNonDuplicateEpisodes()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("1"), 3);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value!.AddedEpisodeCount).IsEqualTo(1); // episode 3
    }

    [Test]
    public async Task AllDuplicateEpisodes_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        // Setup
        await handler.HandleAsync(
            new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("1"), 3)
        );

        // Action
        var command = new AddEpisodeCommand(seed.ProjectId, seed.User1Id, Number.From("1"), 3);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.EpisodeConflict);
    }

    [Test]
    public async Task NoPermissions_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddEpisodeHandler(
            db,
            UserVerificationService,
            NullLogger<AddEpisodeHandler>.Instance
        );

        var command = new AddEpisodeCommand(seed.ProjectId, seed.User2Id, Number.From("1"));
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
