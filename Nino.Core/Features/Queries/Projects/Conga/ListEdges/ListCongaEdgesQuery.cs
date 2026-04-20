// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Projects.Conga.ListEdges;

public sealed record ListCongaEdgesQuery(ProjectId ProjectId) : IQuery;
