// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.CongaReminders.Disable;

namespace Nino.Core.Tests.Commands.Projects.CongaReminders;

public class DisableCongaRemindersHandlerTest : TestBase
{
    [Test]
    public async Task Command_Disables_CongaReminders()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new DisableCongaRemindersHandler(
            db,
            UserVerificationService,
            NullLogger<DisableCongaRemindersHandler>.Instance
        );

        var command = new DisableCongaRemindersCommand(seed.ProjectId, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = db.Projects.First(p => p.Id == command.ProjectId);
        await Assert.That(project.CongaRemindersEnabled).IsFalse();
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new DisableCongaRemindersHandler(
            db,
            UserVerificationService,
            NullLogger<DisableCongaRemindersHandler>.Instance
        );

        var command = new DisableCongaRemindersCommand(seed.ProjectId, seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
