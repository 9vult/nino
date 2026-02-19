// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Entities;

public sealed record Result(ResultStatus Status);

public sealed record Result<T>(ResultStatus Status, T? Value = default);
