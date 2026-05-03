// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Events;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Archive;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects;

public class ArchiveProjectHandlerTests : TestBase
{
    [Test]
    public async Task Command_Archives_Project_And_Publishes_Notifications()
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

        var handler = new ArchiveProjectHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<ArchiveProjectHandler>.Instance
        );

        var command = new ArchiveProjectCommand(seed.ProjectId, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue().Because($"Handler failed: {result.Status}");

        var project = db.Projects.First(t => t.Id == command.ProjectId);

        await Assert.That(project.IsArchived).IsTrue();
        BusImposter.PublishAsync(Arg<ProjectArchivedEvent>.Any()).Called(Count.Once());
        BusImposter.PublishAsync(Arg<ProjectArchivedObserverEvent>.Any()).Called(Count.Once());
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new ArchiveProjectHandler(
            db,
            UserVerificationService,
            EventBus,
            NullLogger<ArchiveProjectHandler>.Instance
        );

        var command = new ArchiveProjectCommand(seed.ProjectId, seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
