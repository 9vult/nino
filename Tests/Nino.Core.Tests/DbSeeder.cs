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
        var user3Id = await identityService.GetOrCreateUserByDiscordIdAsync(1234, "TestUser3");
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

        var episode1 = new Episode
        {
            ProjectId = project.Id,
            GroupId = groupId,
            Number = Number.From("1"),
            IsDone = true,
        };
        project.Episodes.Add(episode1);
        var episode2 = new Episode
        {
            ProjectId = project.Id,
            GroupId = groupId,
            Number = Number.From("2"),
            IsDone = false,
        };
        project.Episodes.Add(episode2);

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

        var template1Task1 = new Task
        {
            EpisodeId = episode1.Id,
            AssigneeId = user1Id,
            Abbreviation = Abbreviation.From("ED"),
            Name = "Editing",
            Weight = 0,
            IsPseudo = false,
            IsDone = true,
        };
        var template2Task1 = new Task
        {
            EpisodeId = episode1.Id,
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("TLC"),
            Name = "Translation Checking",
            Weight = 1,
            IsPseudo = false,
            IsDone = true,
        };
        var additional1Task1 = new Task
        {
            EpisodeId = episode1.Id,
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("STL"),
            Name = "Song Translation",
            Weight = 3,
            IsPseudo = false,
            IsDone = true,
        };
        episode1.Tasks.Add(template1Task1);
        episode1.Tasks.Add(template2Task1);
        episode1.Tasks.Add(additional1Task1);

        var template1Task2 = new Task
        {
            EpisodeId = episode2.Id,
            AssigneeId = user1Id,
            Abbreviation = Abbreviation.From("ED"),
            Name = "Editing",
            Weight = 0,
            IsPseudo = false,
            IsDone = false,
        };
        var template2Task2 = new Task
        {
            EpisodeId = episode2.Id,
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("TLC"),
            Name = "Translation Checking",
            Weight = 1,
            IsPseudo = false,
            IsDone = false,
        };
        var additional1Task2 = new Task
        {
            EpisodeId = episode2.Id,
            AssigneeId = user2Id,
            Abbreviation = Abbreviation.From("KFX"),
            Name = "Song Styling",
            Weight = 3,
            IsPseudo = false,
            IsDone = false,
        };
        episode2.Tasks.Add(template1Task2);
        episode2.Tasks.Add(template2Task2);
        episode2.Tasks.Add(additional1Task2);

        await db.SaveChangesAsync();

        return new SeedInfo(
            user1Id,
            user2Id,
            user3Id,
            groupId,
            project.Id,
            episode1.Id,
            episode2.Id,
            templateStaff1.Id,
            templateStaff2.Id,
            template1Task1.Id,
            template1Task1.Id,
            additional1Task1.Id,
            template1Task2.Id,
            template1Task2.Id,
            additional1Task2.Id
        );
    }
}
