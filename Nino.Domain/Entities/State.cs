// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public class State
{
    public StateId Id { get; set; } = StateId.FromNewGuid();

    [MaxLength(2048)]
    public required string Json { get; set; }
}
