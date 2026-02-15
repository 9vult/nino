// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

public sealed record ActionResult(ActionStatus Status, string Message = "")
{
    public static ActionResult Success => new(ActionStatus.Success);
}
