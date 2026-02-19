// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Dtos;

namespace Nino.Core.Actions.Project.Create;

public sealed record ProjectCreateAction(
    ProjectCreateDto Dto,
    Guid GroupId,
    Guid OwnerId,
    bool OverrideVerification
);
