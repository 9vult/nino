// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.Project.Create;

public sealed record CreateProjectResponse(
    ProjectId ProjectId,
    string ProjectNickname,
    string ProjectTitle,
    int ProjectLength,
    ProjectType ProjectType
);
