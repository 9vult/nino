// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Tasks.GetTaskInfo;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Done;

public sealed record DoneState(
    GetGenericProjectDataResponse ProjectData,
    GetTaskInfoResponse TaskData,
    int AheadCount,
    UserId RequestedBy
);
