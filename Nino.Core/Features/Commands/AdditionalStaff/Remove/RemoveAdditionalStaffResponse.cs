// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.AdditionalStaff.Remove;

public sealed record RemoveAdditionalStaffResponse(
    string ProjectTitle,
    ProjectType ProjectType,
    string PosterUrl,
    bool IsEpisodeComplete
);
