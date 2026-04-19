// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.List;

public sealed record ListTasksResult(TaskId Id, Abbreviation Abbreviation);
