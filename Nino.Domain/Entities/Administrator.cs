// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class Administrator
{
    public AdministratorId Id { get; set; } = AdministratorId.New();
    public UserId UserId { get; set; }
    public User User { get; set; } = null!;
}
