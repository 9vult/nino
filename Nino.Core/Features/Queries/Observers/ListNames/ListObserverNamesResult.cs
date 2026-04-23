// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Observers.ListNames;

public sealed record ListObserverNamesResult(ObserverId Id, string GroupName, Alias Nickname);
