// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class User : MappedId<UserId>
{
    [MaxLength(Length.UserName)]
    public string Name { get; set; } = string.Empty;

    public bool IsSystemAdministrator { get; set; } = false;
}
