// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Admins.Add;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects.Admins;

public class AddProjectAdminHandlerTests : TestBase
{
    [Test]
    public async Task Command_Adds_Admin_To_Project()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddProjectAdminHandler(
            db,
            UserVerificationService,
            NullLogger<AddProjectAdminHandler>.Instance
        );

        var command = new AddProjectAdminCommand(seed.ProjectId, seed.User3Id, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = db.Projects.First();

        await Assert.That(project.Administrators).Contains(a => a.UserId == seed.User3Id);
    }

    [Test]
    public async Task Duplicate_Add_Request_Returns_Error()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var project = db.Projects.First();
        project.Administrators.Add(new Administrator { UserId = seed.User3Id });
        await db.SaveChangesAsync();

        var handler = new AddProjectAdminHandler(
            db,
            UserVerificationService,
            NullLogger<AddProjectAdminHandler>.Instance
        );

        var command = new AddProjectAdminCommand(seed.ProjectId, seed.User3Id, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddProjectAdminHandler(
            db,
            UserVerificationService,
            NullLogger<AddProjectAdminHandler>.Instance
        );

        var command = new AddProjectAdminCommand(seed.ProjectId, seed.User3Id, seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
