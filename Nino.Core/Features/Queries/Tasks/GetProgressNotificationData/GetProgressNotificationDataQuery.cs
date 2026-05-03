// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.GetProgressNotificationData;

public sealed record GetProgressNotificationDataQuery(TaskId TaskId) : IQuery;
