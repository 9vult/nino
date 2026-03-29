// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Edit;

namespace Nino.Core.Tests.Commands.Projects;

public class EditProjectHandlerTests : TestBase
{
    [Test]
    public async Task Command_EditsProject()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        // Original data
        var og = await db
            .Projects.Select(p => new
            {
                p.Nickname,
                p.PosterUrl,
                p.AniListId,
                p.IsPrivate,
                p.UpdateChannelId,
                p.ReleaseChannelId,
            })
            .FirstAsync();

        // Seed
        var pChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(9988);

        var handler = new EditProjectHandler(
            db,
            UserVerificationService,
            NullLogger<EditProjectHandler>.Instance
        );

        var command = new EditProjectCommand(
            seed.ProjectId,
            seed.User1Id,
            Nickname: null,
            Title: "Johnny Phat's Rap Mania",
            Motd: "On hold until Tuesday",
            PosterUrl: null,
            AniListId: null,
            IsPrivate: null,
            ProjectChannelId: pChannelId,
            UpdateChannelId: null,
            ReleaseChannelId: null,
            AniListOffset: 16
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsTrue();

        var project = await db.Projects.FirstAsync();
        await Assert.That(project.Nickname).IsEqualTo(og.Nickname);
        await Assert.That(project.Title).IsEqualTo(command.Title);
        await Assert.That(project.Motd).IsEqualTo(command.Motd);
        await Assert.That(project.PosterUrl).IsEqualTo(og.PosterUrl);
        await Assert.That(project.AniListId).IsEqualTo(og.AniListId);
        await Assert.That(project.IsPrivate).IsEqualTo(og.IsPrivate);
        await Assert.That(project.ProjectChannelId).IsEqualTo(command.ProjectChannelId!.Value);
        await Assert.That(project.UpdateChannelId).IsEqualTo(og.UpdateChannelId);
        await Assert.That(project.ReleaseChannelId).IsEqualTo(og.ReleaseChannelId);
        await Assert.That(project.AniListOffset).IsEqualTo(command.AniListOffset!.Value);
    }

    [Test]
    public async Task Unauthorized_ReturnsUnauthorized()
    {
        var seed = await Db.SeedAsync(IdentityService);
        var db = Db.Context;

        var handler = new EditProjectHandler(
            db,
            UserVerificationService,
            NullLogger<EditProjectHandler>.Instance
        );

        var command = new EditProjectCommand(
            seed.ProjectId,
            seed.User3Id,
            Nickname: null,
            Title: "Johnny Phat's Rap Mania",
            Motd: "On hold until Tuesday",
            PosterUrl: null,
            AniListId: null,
            IsPrivate: null,
            ProjectChannelId: null,
            UpdateChannelId: null,
            ReleaseChannelId: null,
            AniListOffset: 16
        );
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.Unauthorized);
    }
}
