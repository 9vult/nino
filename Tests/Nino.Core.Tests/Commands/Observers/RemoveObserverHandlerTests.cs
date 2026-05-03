// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Events;
using Nino.Core.Features.Commands.Observers.Add;
using Nino.Core.Features.Commands.Observers.Remove;

namespace Nino.Core.Tests.Commands.Observers;

public class RemoveObserverHandlerTests : TestBase
{
    [Test]
    public async Task Observer_IsRemoved()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(121212);
        var channelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(444444);

        // Setup
        var addHandler = new AddObserverHandler(
            db,
            UserVerificationService,
            NullLogger<AddObserverHandler>.Instance
        );
        await addHandler.HandleAsync(
            new AddObserverCommand(
                seed.ProjectId,
                groupId,
                seed.User3Id,
                true,
                channelId,
                channelId
            )
        );

        var observerId = await db.Observers.Select(o => o.Id).FirstAsync();

        var handler = new RemoveObserverHandler(
            db,
            EventBus,
            UserVerificationService,
            NullLogger<RemoveObserverHandler>.Instance
        );

        var command = new RemoveObserverCommand(observerId, seed.User3Id, true);

        var result = await handler.HandleAsync(command);
        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var observers = await db.Observers.Where(o => o.ProjectId == seed.ProjectId).ToListAsync();

        await Assert.That(observers).IsEmpty();
    }

    [Test]
    public async Task DelegateObserver_IsRemoved_And_Fires_Event()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(121212);
        var channelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(444444);

        // Setup
        var addHandler = new AddObserverHandler(
            db,
            UserVerificationService,
            NullLogger<AddObserverHandler>.Instance
        );
        await addHandler.HandleAsync(
            new AddObserverCommand(
                seed.ProjectId,
                groupId,
                seed.User3Id,
                true,
                channelId,
                channelId
            )
        );
        var observerId = await db.Observers.Select(o => o.Id).FirstAsync();
        var project = await db.Projects.FirstAsync();
        project.DelegateObserverId = observerId;
        await db.SaveChangesAsync();

        var handler = new RemoveObserverHandler(
            db,
            EventBus,
            UserVerificationService,
            NullLogger<RemoveObserverHandler>.Instance
        );

        var command = new RemoveObserverCommand(observerId, seed.User3Id, true);

        var result = await handler.HandleAsync(command);
        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var observers = await db.Observers.Where(o => o.ProjectId == seed.ProjectId).ToListAsync();
        await Assert.That(observers).IsEmpty();

        project = await db.Projects.FirstAsync();
        await Assert.That(project.DelegateObserverId).IsNull();
        BusImposter.PublishAsync(Arg<DelegateObserverDeletedEvent>.Any()).Called(Count.Once());
    }
}
