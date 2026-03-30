// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Groups.Admins.Remove;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Groups.Admins;

public class RemoveGroupAdminHandlerTests : TestBase
{
    [Test]
    public async Task Command_Removes_Admin_From_Group()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var config = (await db.Groups.FirstAsync()).Configuration;
        config.Administrators.Add(new Administrator { UserId = seed.User3Id });
        await db.SaveChangesAsync();

        var handler = new RemoveGroupAdminHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveGroupAdminHandler>.Instance
        );

        var command = new RemoveGroupAdminCommand(seed.GroupId, seed.User3Id, seed.User1Id, true);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        config = (await db.Groups.FirstAsync()).Configuration;

        await Assert.That(config.Administrators).DoesNotContain(a => a.UserId == seed.User1Id);
    }

    [Test]
    public async Task Nonexistent_Remove_Request_Returns_Error()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveGroupAdminHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveGroupAdminHandler>.Instance
        );

        var command = new RemoveGroupAdminCommand(seed.GroupId, seed.User3Id, seed.User1Id, true);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new RemoveGroupAdminHandler(
            db,
            UserVerificationService,
            NullLogger<RemoveGroupAdminHandler>.Instance
        );

        var command = new RemoveGroupAdminCommand(seed.GroupId, seed.User3Id, seed.User3Id, false);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
