// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Actions.Project.Create;

public sealed record ProjectCreateResult(
    ActionStatus Status,
    Guid? ProjectId,
    string ProjectNickname
);
