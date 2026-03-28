// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.TemplateStaff.Remove;

public sealed record RemoveTemplateStaffResponse(List<(EpisodeId, Number)> CompletedEpisodes);
