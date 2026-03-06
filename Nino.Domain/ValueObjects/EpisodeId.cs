// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public readonly record struct EpisodeId(Guid Value)
{
    public static EpisodeId New() => new(Guid.NewGuid());

    public static EpisodeId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
