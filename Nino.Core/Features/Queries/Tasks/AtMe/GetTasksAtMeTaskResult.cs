// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed record GetTasksAtMeTaskResult(Abbreviation Abbreviation, decimal Weight, bool IsPseudo);
