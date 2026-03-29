// SPDX-License-Identifier: MPL-2.0

using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.CongaReminders.Enable;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Commands.Projects.CongaReminders;

public class EnableCongaRemindersHandlerTest : TestBase
{
    [Test]
    public async Task Command_Enables_CongaReminders()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EnableCongaRemindersHandler(
            db,
            UserVerificationService,
            NullLogger<EnableCongaRemindersHandler>.Instance
        );

        var command = new EnableCongaRemindersCommand(
            seed.ProjectId,
            TimeSpan.FromDays(2),
            seed.User1Id
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = db.Projects.First(p => p.Id == command.ProjectId);
        await Assert.That(project.CongaRemindersEnabled).IsTrue();
        await Assert.That(project.CongaReminderPeriod).IsEqualTo(TimeSpan.FromDays(2));
    }

    [Test]
    public async Task No_Project_Channel_ReturnsError()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var project = db.Projects.First(p => p.Id == seed.ProjectId);
        project.ProjectChannelId = ChannelId.Unset;
        await db.SaveChangesAsync();

        var handler = new EnableCongaRemindersHandler(
            db,
            UserVerificationService,
            NullLogger<EnableCongaRemindersHandler>.Instance
        );

        var command = new EnableCongaRemindersCommand(
            seed.ProjectId,
            TimeSpan.FromDays(2),
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

        var handler = new EnableCongaRemindersHandler(
            db,
            UserVerificationService,
            NullLogger<EnableCongaRemindersHandler>.Instance
        );

        var command = new EnableCongaRemindersCommand(
            seed.ProjectId,
            TimeSpan.FromDays(2),
            seed.User3Id
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
