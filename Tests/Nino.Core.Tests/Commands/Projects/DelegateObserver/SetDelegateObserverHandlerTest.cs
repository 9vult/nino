// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.DelegateObserver.Set;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects.DelegateObserver;

public class SetDelegateObserverHandlerTest : TestBase
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
        await db.SaveChangesAsync();

        var handler = new SetDelegateObserverHandler(
            db,
            UserVerificationService,
            NullLogger<SetDelegateObserverHandler>.Instance
        );

        var command = new SetDelegateObserverCommand(seed.ProjectId, observer.Id, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = db.Projects.First(p => p.Id == command.ProjectId);
        await Assert.That(project.DelegateObserverId).IsEqualTo(observer.Id);
    }

    [Test]
    public async Task No_Project_Channel_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var project = db.Projects.First(p => p.Id == seed.ProjectId);
        project.ProjectChannelId = ChannelId.Unset;
        await db.SaveChangesAsync();

        var handler = new SetDelegateObserverHandler(
            db,
            UserVerificationService,
            NullLogger<SetDelegateObserverHandler>.Instance
        );

        var command = new SetDelegateObserverCommand(
            seed.ProjectId,
            ObserverId.FromNewGuid(),
            seed.User1Id
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.MissingProjectChannel);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new SetDelegateObserverHandler(
            db,
            UserVerificationService,
            NullLogger<SetDelegateObserverHandler>.Instance
        );

        var command = new SetDelegateObserverCommand(
            seed.ProjectId,
            ObserverId.FromNewGuid(),
            seed.User3Id
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
