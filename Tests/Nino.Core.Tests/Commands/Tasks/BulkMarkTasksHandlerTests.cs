// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Events;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.BulkMark;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Tasks;

public class BulkMarkTasksHandlerTests : TestBase
{
    [Test]
    public async Task Done_Command_Marks_Task_Done_And_Publishes_Notifications()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup - add an observer
        var observer = new Observer
        {
            GroupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(9911),
            OriginGroupId = seed.GroupId,
            OwnerId = seed.User3Id,
            ProjectId = seed.ProjectId,
            UpdateChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(9911),
            ReleaseChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1199),
        };
        await db.Observers.AddAsync(observer);
        await db.SaveChangesAsync();

        var handler = new BulkMarkTasksHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<BulkMarkTasksHandler>.Instance
        );

        var command = new BulkMarkTasksCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            ProgressType.Done,
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var task1 = await db.Tasks.FirstAsync(t =>
            t.EpisodeId == seed.Episode1Id && t.Abbreviation == Abbreviation.From("ED")
        );
        var task2 = await db.Tasks.FirstAsync(t =>
            t.EpisodeId == seed.Episode2Id && t.Abbreviation == Abbreviation.From("ED")
        );

        await Assert.That(task1.IsDone).IsTrue();
        await Assert.That(task2.IsDone).IsTrue();

        BusImposter.PublishAsync(Arg<BulkTaskProgressEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<BulkTaskProgressObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Done_Command_Marks_Task_Skipped_And_Publishes_Notifications()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup - add an observer
        var observer = new Observer
        {
            GroupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(9911),
            OriginGroupId = seed.GroupId,
            OwnerId = seed.User3Id,
            ProjectId = seed.ProjectId,
            UpdateChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(9911),
            ReleaseChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1199),
        };
        await db.Observers.AddAsync(observer);
        await db.SaveChangesAsync();

        var handler = new BulkMarkTasksHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<BulkMarkTasksHandler>.Instance
        );

        var command = new BulkMarkTasksCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            ProgressType.Skipped,
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var task1 = await db.Tasks.FirstAsync(t =>
            t.EpisodeId == seed.Episode1Id && t.Abbreviation == Abbreviation.From("ED")
        );
        var task2 = await db.Tasks.FirstAsync(t =>
            t.EpisodeId == seed.Episode2Id && t.Abbreviation == Abbreviation.From("ED")
        );

        await Assert.That(task1.IsDone).IsTrue();
        await Assert.That(task2.IsDone).IsTrue();

        BusImposter.PublishAsync(Arg<BulkTaskProgressEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<BulkTaskProgressObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Done_Command_Marks_Task_Undone_And_Publishes_Notifications()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup - add an observer
        var observer = new Observer
        {
            GroupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(9911),
            OriginGroupId = seed.GroupId,
            OwnerId = seed.User3Id,
            ProjectId = seed.ProjectId,
            UpdateChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(9911),
            ReleaseChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1199),
        };
        await db.Observers.AddAsync(observer);
        await db.SaveChangesAsync();

        var handler = new BulkMarkTasksHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<BulkMarkTasksHandler>.Instance
        );

        var command = new BulkMarkTasksCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            ProgressType.Undone,
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var task1 = await db.Tasks.FirstAsync(t =>
            t.EpisodeId == seed.Episode1Id && t.Abbreviation == Abbreviation.From("ED")
        );
        var task2 = await db.Tasks.FirstAsync(t =>
            t.EpisodeId == seed.Episode2Id && t.Abbreviation == Abbreviation.From("ED")
        );

        await Assert.That(task1.IsDone).IsFalse();
        await Assert.That(task2.IsDone).IsFalse();

        BusImposter.PublishAsync(Arg<BulkTaskProgressEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<BulkTaskProgressObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new BulkMarkTasksHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<BulkMarkTasksHandler>.Instance
        );

        var command = new BulkMarkTasksCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Episode2Id,
            Abbreviation.From("ED"),
            ProgressType.Undone,
            seed.User3Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
