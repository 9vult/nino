// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;

namespace Nino.Core.Dtos;

public sealed class TaskProgressDto
{
    public required string Abbreviation { get; init; }
    public required string FullName { get; init; }
    public required ProgressResponseType ProgressResponseType { get; init; }
    public required ProgressPublishType ProgressPublishType { get; init; }
}
