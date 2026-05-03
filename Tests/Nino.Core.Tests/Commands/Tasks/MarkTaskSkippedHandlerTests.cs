// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Events;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Tasks.MarkDone;
using Nino.Core.Features.Commands.Tasks.MarkSkipped;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Tasks;

public class MarkTaskSkippedHandlerTests : TestBase
{
    [Test]
    public async Task Assignee_Command_Marks_Task_Done_And_Publishes_Notifications()
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

        var handler = new MarkTaskSkippedHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<MarkTaskSkippedHandler>.Instance
        );

        var command = new MarkTaskSkippedCommand(
            seed.ProjectId,
            seed.Episode2Id,
            seed.Task2Id2,
            seed.User2Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var task = db.Tasks.First(t => t.Id == command.TaskId);

        await Assert.That(task.IsDone).IsTrue();
        BusImposter.PublishAsync(Arg<TaskProgressEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<TaskProgressObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Admin_Command_Marks_Task_Done_And_Publishes_Notifications()
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

        var handler = new MarkTaskSkippedHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<MarkTaskSkippedHandler>.Instance
        );

        var command = new MarkTaskSkippedCommand(
            seed.ProjectId,
            seed.Episode2Id,
            seed.Task2Id2,
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var task = db.Tasks.First(t => t.Id == command.TaskId);

        await Assert.That(task.IsDone).IsTrue();
        BusImposter.PublishAsync(Arg<TaskProgressEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<TaskProgressObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new MarkTaskSkippedHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<MarkTaskSkippedHandler>.Instance
        );

        var command = new MarkTaskSkippedCommand(
            seed.ProjectId,
            seed.Episode2Id,
            seed.Task2Id2,
            seed.User3Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }

    [Test]
    public async Task Completed_Task_ReturnsBadRequest()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new MarkTaskSkippedHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<MarkTaskSkippedHandler>.Instance
        );

        var command = new MarkTaskSkippedCommand(
            seed.ProjectId,
            seed.Episode1Id,
            seed.Task2Id1,
            seed.User1Id
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
    }
}
