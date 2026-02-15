// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Actions.Project.Resolve;

public sealed record ProjectResolveResult(ActionStatus Status, Guid? ProjectId);
