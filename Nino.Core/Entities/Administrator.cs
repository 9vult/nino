// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

/// <summary>
/// Users with administrator permissions, either to a <see cref="Project"/> or a server (via <see cref="Configuration"/>
/// </summary>
public class Administrator
{
    [Key]
    public Guid Id { get; set; }
    public ulong UserId { get; set; }
}
