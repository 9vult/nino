// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests;

public record SeedInfo(
    UserId User1Id,
    UserId User2Id,
    GroupId GroupId,
    ProjectId ProjectId,
    EpisodeId EpisodeId,
    TemplateStaffId TemplateStaff1Id,
    TemplateStaffId TemplateStaff2Id,
    TaskId Task1Id,
    TaskId Task2Id,
    TaskId Task3Id
);
