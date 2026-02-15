// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Actions.Project.Delete;

public sealed record ProjectDeleteResult(ActionStatus Status, string? Json);
