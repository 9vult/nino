// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct TaskId(Guid Value) : IId<TaskId>
{
    public static TaskId New() => new(Guid.NewGuid());

    public static TaskId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
