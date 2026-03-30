// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Groups.Admins.Add;
using Nino.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Groups.Admins;

public class AddGroupAdminHandlerTests : TestBase
{
    [Test]
    public async Task Command_Adds_Admin_To_Group()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddGroupAdminHandler(
            db,
            UserVerificationService,
            NullLogger<AddGroupAdminHandler>.Instance
        );

        var command = new AddGroupAdminCommand(seed.GroupId, seed.User3Id, seed.User1Id, true);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var config = (await db.Groups.FirstAsync()).Configuration;

        await Assert.That(config.Administrators).Contains(a => a.UserId == seed.User3Id);
    }

    [Test]
    public async Task Duplicate_Add_Request_Returns_Error()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var config = (await db.Groups.FirstAsync()).Configuration;
        config.Administrators.Add(new Administrator { UserId = seed.User3Id });
        await db.SaveChangesAsync();

        var handler = new AddGroupAdminHandler(
            db,
            UserVerificationService,
            NullLogger<AddGroupAdminHandler>.Instance
        );

        var command = new AddGroupAdminCommand(seed.GroupId, seed.User3Id, seed.User1Id, true);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.BadRequest);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new AddGroupAdminHandler(
            db,
            UserVerificationService,
            NullLogger<AddGroupAdminHandler>.Instance
        );

        var command = new AddGroupAdminCommand(seed.GroupId, seed.User3Id, seed.User3Id, false);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
