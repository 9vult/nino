// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Delete;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects;

public class DeleteProjectHandlerTests : TestBase
{
    [Test]
    public async Task Command_Deletes_Project_And_Related_Data()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Setup
        var observer = new Observer
        {
            GroupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(9911),
            OriginGroupId = seed.GroupId,
            OwnerId = seed.User3Id,
            ProjectId = seed.ProjectId,
            UpdateChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(9119),
            ReleaseChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1199),
        };
        await db.Observers.AddAsync(observer);
        await db.SaveChangesAsync();

        var handler = new DeleteProjectHandler(
            db,
            UserVerificationService,
            NullLogger<DeleteProjectHandler>.Instance
        );

        var command = new DeleteProjectCommand(seed.ProjectId, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        // Because we only seed a single project, we can use IsEmpty
        await Assert.That(db.Projects).IsEmpty();
        await Assert.That(db.Episodes).IsEmpty();
        await Assert.That(db.Tasks).IsEmpty();
        await Assert.That(db.TemplateStaff).IsEmpty();
        await Assert.That(db.Observers).IsEmpty();
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new DeleteProjectHandler(
            db,
            UserVerificationService,
            NullLogger<DeleteProjectHandler>.Instance
        );

        var command = new DeleteProjectCommand(seed.ProjectId, seed.User2Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
