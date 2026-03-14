// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Remove;

public sealed record RemoveKeyStaffResponse(
    string ProjectTitle,
    ProjectType ProjectType,
    string PosterUrl,
    List<(EpisodeId, string)> CompletedEpisodes
);
