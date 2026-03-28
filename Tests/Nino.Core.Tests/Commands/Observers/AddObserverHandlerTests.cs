// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features.Commands.Observers.Add;

namespace Nino.Core.Tests.Commands.Observers;

public class AddObserverHandlerTests : TestBase
{
    [Test]
    public async Task NewObserver_IsAdded()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(121212);
        var channelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(444444);

        var handler = new AddObserverHandler(
            db,
            UserVerificationService,
            NullLogger<AddObserverHandler>.Instance
        );

        var command = new AddObserverCommand(
            seed.ProjectId,
            groupId,
            seed.User3Id,
            channelId,
            channelId
        );

        var result = await handler.HandleAsync(command);
        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var observer = await db
            .Observers.Where(o => o.ProjectId == seed.ProjectId)
            .FirstOrDefaultAsync();

        await Assert.That(observer).IsNotNull();
        await Assert.That(observer.GroupId).IsEqualTo(groupId);
        await Assert.That(observer.OriginGroupId).IsEqualTo(seed.GroupId);
        await Assert.That(observer.ReleaseChannelId).IsEqualTo(channelId);
    }

    [Test]
    public async Task ExistingObserver_IsUpdated()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(121212);
        var channelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(444444);
        var roleId = await IdentityService.GetOrCreateRoleByDiscordIdAsync(444444);

        var handler = new AddObserverHandler(
            db,
            UserVerificationService,
            NullLogger<AddObserverHandler>.Instance
        );

        // Setup
        await handler.HandleAsync(
            new AddObserverCommand(seed.ProjectId, groupId, seed.User3Id, channelId, channelId)
        );

        var command = new AddObserverCommand(
            seed.ProjectId,
            groupId,
            seed.User3Id,
            channelId,
            channelId,
            PrimaryRoleId: roleId
        );

        var result = await handler.HandleAsync(command);
        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var observers = await db.Observers.Where(o => o.ProjectId == seed.ProjectId).ToListAsync();

        await Assert.That(observers).HasSingleItem(); // Updated, not added

        var observer = observers.First();
        await Assert.That(observer).IsNotNull();
        await Assert.That(observer.GroupId).IsEqualTo(groupId);
        await Assert.That(observer.OriginGroupId).IsEqualTo(seed.GroupId);
        await Assert.That(observer.ReleaseChannelId).IsEqualTo(channelId);
        await Assert.That(observer.ReleaseChannelId).IsEqualTo(channelId);
        await Assert.That(observer.PrimaryRoleId).IsEqualTo(roleId);
    }
}
