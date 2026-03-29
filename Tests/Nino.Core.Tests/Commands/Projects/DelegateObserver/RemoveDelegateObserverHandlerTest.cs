// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.AirNotifications.Enable;
using Nino.Core.Features.Commands.Projects.DelegateObserver.Remove;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects.DelegateObserver;

public class RemoveDelegateObserverHandlerTest : TestBase
{
    [Test]
    public async Task Command_Sets_Delegate_Observer()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup
        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(9911);
        var channelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(9911);
        var observer = new Observer
        {
            GroupId = groupId,
            OriginGroupId = seed.GroupId,
            OwnerId = seed.User3Id,
            ProjectId = seed.ProjectId,
            UpdateChannelId = channelId,
            ReleaseChannelId = channelId,
        };
        await db.Observers.AddAsync(observer);
        var project = db.Projects.First(p => p.Id == observer.ProjectId);
        project.DelegateObserverId = observer.Id;
        await db.SaveChangesAsync();

        var handler = new RemoveDelegateObserverHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveDelegateObserverHandler>.Instance
        );

        var command = new RemoveDelegateObserverCommand(seed.ProjectId, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        project = db.Projects.First(p => p.Id == command.ProjectId);
        await Assert.That(project.DelegateObserverId).IsNull();
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveDelegateObserverHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveDelegateObserverHandler>.Instance
        );

        var command = new RemoveDelegateObserverCommand(seed.ProjectId, seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
