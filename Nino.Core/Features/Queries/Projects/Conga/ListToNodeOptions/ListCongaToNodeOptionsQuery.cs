// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Conga.ListToNodeOptions;

public sealed record ListCongaToNodeOptionsQuery(ProjectId ProjectId) : IQuery;
