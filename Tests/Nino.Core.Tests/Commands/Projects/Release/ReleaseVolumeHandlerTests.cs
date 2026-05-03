// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Events;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Release.Volume;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects.Release;

public class ReleaseVolumeHandlerTests : TestBase
{
    [Test]
    public async Task Command_Publishes_Notifications()
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

        var handler = new ReleaseVolumeHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<ReleaseVolumeHandler>.Instance
        );

        var command = new ReleaseVolumeCommand(
            seed.ProjectId,
            seed.User1Id,
            Number.From("1v2"),
            []
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        BusImposter.PublishAsync(Arg<VolumeReleasedEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<VolumeReleasedObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new ReleaseVolumeHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<ReleaseVolumeHandler>.Instance
        );

        var command = new ReleaseVolumeCommand(seed.ProjectId, seed.User3Id, Number.From("1"), []);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
