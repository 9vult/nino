// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Projects.Create;
using Nino.Core.Services;
using Nino.Domain.Dtos.AniList;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Tests.Commands.Projects;

public class CreateProjectHandlerTests : TestBase
{
    [Test]
    public async Task FullCommand_CreatesProject()
    {
        // Seed
        var userId = await IdentityService.GetOrCreateUserByDiscordIdAsync(1234, "TestUser");
        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(1234);
        var pChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1111);
        var uChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(2222);
        var rChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(3333);

        var aniListService = IAniListService.Imposter();
        aniListService
            .GetAnimeAsync(Arg<AniListId>.Any())
            .ReturnsAsync(Result<AniListResponse>.Success(null!));

        var handler = new CreateProjectHandler(
            Db.Context,
            aniListService.Instance(),
            UserVerificationService,
            NullLogger<CreateProjectHandler>.Instance
        );

        var result = await handler.HandleAsync(
            new CreateProjectCommand(
                groupId,
                userId,
                true,
                Alias.From("test"),
                AniListId.From(1),
                false,
                pChannelId,
                uChannelId,
                rChannelId,
                "Test Project",
                ProjectType.TV,
                6,
                "https://example.com/test.png"
            )
        );

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(await Db.Context.Projects.CountAsync()).IsEqualTo(1);
        await Assert.That(await Db.Context.Episodes.CountAsync()).IsEqualTo(6);
    }

    [Test]
    public async Task PartialCommand_CreatesProject()
    {
        // Seed
        var userId = await IdentityService.GetOrCreateUserByDiscordIdAsync(1234, "TestUser");
        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(1234);
        var pChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1111);
        var uChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(2222);
        var rChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(3333);

        var aniListService = IAniListService.Imposter();
        aniListService
            .GetAnimeAsync(Arg<AniListId>.Any())
            .ReturnsAsync(
                Result<AniListResponse>.Success(
                    new AniListResponse
                    {
                        Id = AniListId.From(1),
                        Data = new AniListRoot
                        {
                            Data = new Data
                            {
                                Media = new Media
                                {
                                    Title = new Title { Romaji = "Test Project" },
                                    Episodes = 6,
                                },
                            },
                        },
                        FetchedAt = default,
                    }
                )
            );

        var handler = new CreateProjectHandler(
            Db.Context,
            aniListService.Instance(),
            UserVerificationService,
            NullLogger<CreateProjectHandler>.Instance
        );

        var result = await handler.HandleAsync(
            new CreateProjectCommand(
                groupId,
                userId,
                true,
                Alias.From("test"),
                AniListId.From(1),
                false,
                pChannelId,
                uChannelId,
                rChannelId,
                null,
                ProjectType.TV,
                null,
                "https://example.com/test.png"
            )
        );

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert
            .That(await Db.Context.Projects.FirstOrDefaultAsync(p => p.Title == "Test Project"))
            .IsNotNull();
        await Assert.That(await Db.Context.Episodes.CountAsync()).IsEqualTo(6);
    }

    [Test]
    public async Task DuplicateCommand_ReturnsConflict()
    {
        // Seed
        var userId = await IdentityService.GetOrCreateUserByDiscordIdAsync(1234, "TestUser");
        var groupId = await IdentityService.GetOrCreateGroupByDiscordIdAsync(1234);
        var pChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(1111);
        var uChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(2222);
        var rChannelId = await IdentityService.GetOrCreateChannelByDiscordIdAsync(3333);

        var aniListService = IAniListService.Imposter();
        aniListService
            .GetAnimeAsync(Arg<AniListId>.Any())
            .ReturnsAsync(Result<AniListResponse>.Success(null!));

        var handler = new CreateProjectHandler(
            Db.Context,
            aniListService.Instance(),
            UserVerificationService,
            NullLogger<CreateProjectHandler>.Instance
        );

        var command = new CreateProjectCommand(
            groupId,
            userId,
            true,
            Alias.From("test"),
            AniListId.From(1),
            false,
            pChannelId,
            uChannelId,
            rChannelId,
            "Test Project",
            ProjectType.TV,
            6,
            "https://example.com/test.png"
        );

        await handler.HandleAsync(command);
        var result = await handler.HandleAsync(command);

        await Assert.That(result.IsSuccess).IsFalse();
        await Assert.That(result.Status).IsEqualTo(ResultStatus.ProjectConflict);
        await Assert.That(await Db.Context.Projects.CountAsync()).IsEqualTo(1);
        await Assert.That(await Db.Context.Episodes.CountAsync()).IsEqualTo(6);
    }
}
