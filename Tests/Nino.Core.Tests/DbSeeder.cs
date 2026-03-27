// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Tests;

public static class DbSeeder
{
    /// <summary>
    /// Seed the database with a project, episode, and tasks
    /// </summary>
    /// <param name="testDb">Test database</param>
    /// <param name="identityService">Identity Service for creating users, etc.</param>
    /// <returns>A bunch of IDs</returns>
    public static async Task<SeedInfo> SeedAsync(
        this TestDatabase testDb,
        IIdentityService identityService
    )
    {
        var db = testDb.Context;

        var user1Id = await identityService.GetOrCreateUserByDiscordIdAsync(1234, "TestUser1");
        var user2Id = await identityService.GetOrCreateUserByDiscordIdAsync(5678, "TestUser2");
        var groupId = await identityService.GetOrCreateGroupByDiscordIdAsync(1234);
        var pChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(1111);
        var uChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(2222);
        var rChannelId = await identityService.GetOrCreateChannelByDiscordIdAsync(3333);

        var project = new Project
        {
            GroupId = groupId,
            OwnerId = user1Id,
            Type = ProjectType.TV,
            Nickname = Alias.From("test"),
            Title = "Test Project",
            PosterUrl = string.Empty,
            AniListId = AniListId.From(1),
            ProjectChannelId = pChannelId,
            UpdateChannelId = uChannelId,
            ReleaseChannelId = rChannelId,
            IsPrivate = false,
        };
        await db.Projects.AddAsync(project);

        var episode = new Episode
        {
            ProjectId = project.Id,
            GroupId = groupId,
            Number = Number.From("1"),
            IsDone = false,
        };
        project.Episodes.Add(episode);

        var templateStaff1 = new TemplateStaff
        {
            AssigneeId = user1Id,
            Abbreviation = Abbreviation.From("ED"),
            Name = "Editing",
            Weight = 0,
            IsPseudo = false,
            ProjectId = project.Id,
        };
        project.TemplateStaff.Add(templateStaff1);

        var templateStaff2 = new TemplateStaff
        {
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("TLC"),
            Name = "Translation Checking",
            Weight = 1,
            IsPseudo = false,
            ProjectId = project.Id,
        };
        project.TemplateStaff.Add(templateStaff2);

        var templateTask1 = new Task
        {
            EpisodeId = episode.Id,
            AssigneeId = user1Id,
            Abbreviation = Abbreviation.From("ED"),
            Name = "Editing",
            Weight = 0,
            IsPseudo = false,
            IsDone = false,
        };
        var templateTask2 = new Task
        {
            EpisodeId = episode.Id,
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("TLC"),
            Name = "Translation Checking",
            Weight = 1,
            IsPseudo = false,
            IsDone = false,
        };

        var additionalTask = new Task
        {
            EpisodeId = episode.Id,
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("KFX"),
            Name = "Song Styling",
            Weight = 3,
            IsPseudo = false,
            IsDone = false,
        };
        episode.Tasks.Add(templateTask1);
        episode.Tasks.Add(templateTask2);
        episode.Tasks.Add(additionalTask);

        await db.SaveChangesAsync();

        return new SeedInfo(
            user1Id,
            user2Id,
            groupId,
            project.Id,
            episode.Id,
            templateStaff1.Id,
            templateStaff2.Id,
            templateTask1.Id,
            templateTask2.Id,
            additionalTask.Id
        );
    }
}
