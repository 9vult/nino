// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Actions;
using Nino.Core.Enums;

namespace Nino.Core.Entities;

public sealed class Result<T>(ResultStatus status, T? data)
{
    public ResultStatus Status { get; } = status;
    public T? Data { get; } = data;

    public bool IsSuccess => Status == ResultStatus.Success;
}
