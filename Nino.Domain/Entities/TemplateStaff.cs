// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Nino.Domain.ValueObjects;

namespace Nino.Domain.Entities;

public sealed class TemplateStaff
{
    public TemplateStaffId Id { get; set; } = TemplateStaffId.FromNewGuid();

    public required UserId AssigneeId { get; set; }
    public User Assignee { get; set; } = null!;

    public required ProjectId ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [MaxLength(Length.Abbreviation)]
    public required Abbreviation Abbreviation { get; set; }

    [MaxLength(Length.RoleName)]
    public required string Name { get; set; }

    public required decimal Weight { get; set; }

    public required bool IsPseudo { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
