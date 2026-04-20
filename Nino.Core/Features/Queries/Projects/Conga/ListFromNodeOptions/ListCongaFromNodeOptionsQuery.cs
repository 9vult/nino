// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Conga.ListFromNodeOptions;

public sealed record ListCongaFromNodeOptionsQuery(ProjectId ProjectId) : IQuery;
