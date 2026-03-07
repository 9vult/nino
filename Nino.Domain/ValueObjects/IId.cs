// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.ValueObjects;

public interface IId<out TSelf>
    where TSelf : struct, IId<TSelf>
{
    static abstract TSelf New();
    static abstract TSelf From(Guid value);
    Guid Value { get; init; }
}
