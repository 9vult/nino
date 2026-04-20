// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Groups.Edit;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Core.Tests.Commands.Groups;

public class EditGroupHandlerTests : TestBase
{
    [Test]
    public async Task Command_EditsProject()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditGroupHandler(
            db,
            UserVerificationService,
            NullLogger<EditGroupHandler>.Instance
        );

        var command = new EditGroupCommand(
            seed.GroupId,
            seed.User1Id,
            true,
            Locale.Polish,
            PublishPrivateProgress: null,
            null,
            null,
            CongaPrefixType.Title
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var config = (await db.Groups.FirstAsync()).Configuration;

        await Assert.That(config.Locale).IsEqualTo(Locale.Polish);
        await Assert.That(config.CongaPrefixType).IsEqualTo(CongaPrefixType.Title);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditGroupHandler(
            db,
            UserVerificationService,
            NullLogger<EditGroupHandler>.Instance
        );

        var command = new EditGroupCommand(
            seed.GroupId,
            seed.User3Id,
            false,
            Locale.Polish,
            PublishPrivateProgress: null,
            null,
            null,
            CongaPrefixType.Title
        );

        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
