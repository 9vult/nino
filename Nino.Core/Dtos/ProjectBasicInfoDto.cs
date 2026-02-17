// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Dtos;

public sealed record ProjectBasicInfoDto(
    string Nickname,
    string Title,
    ProjectType Type,
    bool IsPrivate
);
