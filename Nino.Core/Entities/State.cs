// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Entities;

public class State
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(2048)]
    public required string Json { get; set; }
}
