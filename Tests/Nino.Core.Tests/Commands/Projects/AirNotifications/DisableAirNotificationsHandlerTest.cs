// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.AirNotifications.Disable;

namespace Nino.Core.Tests.Commands.Projects.AirNotifications;

public class DisableAirNotificationsHandlerTest : TestBase
{
    [Test]
    public async Task Command_Disables_AirNotifications()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new DisableAirNotificationsHandler(
            db,
            UserVerificationService,
            NullLogger<DisableAirNotificationsHandler>.Instance
        );

        var command = new DisableAirNotificationsCommand(seed.ProjectId, seed.User1Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = db.Projects.First(p => p.Id == command.ProjectId);
        await Assert.That(project.AirNotificationsEnabled).IsFalse();
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new DisableAirNotificationsHandler(
            db,
            UserVerificationService,
            NullLogger<DisableAirNotificationsHandler>.Instance
        );

        var command = new DisableAirNotificationsCommand(seed.ProjectId, seed.User3Id);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
