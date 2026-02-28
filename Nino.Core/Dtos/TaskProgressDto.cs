// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Enums;
using Nino.Localization;

namespace Nino.Core.Dtos;

public sealed class TaskProgressDto
{
    public required string EpisodeNumber { get; init; }
    public required string Abbreviation { get; init; }
    public required string FullName { get; init; }
    public required MappedIdDto UpdateChannel { get; init; }
    public required ProgressResponseType ProgressResponseType { get; init; }
    public required ProgressPublishType ProgressPublishType { get; init; }
    public required Locale Locale { get; init; }
}
