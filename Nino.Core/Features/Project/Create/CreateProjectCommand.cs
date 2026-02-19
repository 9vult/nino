// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;

namespace Nino.Core.Features.Project.Create;

public sealed record CreateProjectCommand(
    ProjectCreateDto Dto,
    Guid GroupId,
    Guid OwnerId,
    bool OverrideVerification
);
