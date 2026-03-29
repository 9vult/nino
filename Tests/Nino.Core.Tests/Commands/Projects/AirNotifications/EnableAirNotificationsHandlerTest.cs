// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.AirNotifications.Enable;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Projects.AirNotifications;

public class EnableAirNotificationsHandlerTest : TestBase
{
    [Test]
    public async Task Command_Enables_AirNotifications()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EnableAirNotificationsHandler(
            db,
            UserVerificationService,
            NullLogger<EnableAirNotificationsHandler>.Instance
        );

        var command = new EnableAirNotificationsCommand(
            seed.ProjectId,
            seed.User1Id,
            seed.User2Id,
            null,
            TimeSpan.FromMinutes(45)
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = db.Projects.First(p => p.Id == command.ProjectId);
        await Assert.That(project.AirNotificationsEnabled).IsTrue();
        await Assert.That(project.AirNotificationUserId).IsEqualTo(seed.User2Id);
        await Assert.That(project.AirNotificationRoleId).IsNull();
        await Assert.That(project.AirNotificationDelay).IsEqualTo(TimeSpan.FromMinutes(45));
    }

    [Test]
    public async Task No_Project_Channel_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var project = db.Projects.First(p => p.Id == seed.ProjectId);
        project.ProjectChannelId = ChannelId.Unset;
        await db.SaveChangesAsync();

        var handler = new EnableAirNotificationsHandler(
            db,
            UserVerificationService,
            NullLogger<EnableAirNotificationsHandler>.Instance
        );

        var command = new EnableAirNotificationsCommand(
            seed.ProjectId,
            seed.User1Id,
            seed.User2Id,
            null,
            TimeSpan.FromMinutes(45)
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

        var handler = new EnableAirNotificationsHandler(
            db,
            UserVerificationService,
            NullLogger<EnableAirNotificationsHandler>.Instance
        );

        var command = new EnableAirNotificationsCommand(
            seed.ProjectId,
            seed.User3Id,
            seed.User2Id,
            null,
            TimeSpan.FromMinutes(45)
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
