// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests;

public record SeedInfo(
    UserId User1Id,
    UserId User2Id,
    UserId User3Id,
    GroupId GroupId,
    ProjectId ProjectId,
    EpisodeId Episode1Id,
    EpisodeId Episode2Id,
    TemplateStaffId TemplateStaff1Id,
    TemplateStaffId TemplateStaff2Id,
    TaskId Task1Id1,
    TaskId Task2Id1,
    TaskId Task3Id1,
    TaskId Task1Id2,
    TaskId Task2Id2,
    TaskId Task3Id2
);
