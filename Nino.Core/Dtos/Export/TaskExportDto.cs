// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Dtos.Export;

public sealed class TaskExportDto
{
    public required string Abbreviation { get; init; }
    public required bool IsDone { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastRemindedAt { get; set; }

    internal static TaskExportDto FromTask(Nino.Core.Entities.Task task)
    {
        return new TaskExportDto
        {
            Abbreviation = task.Abbreviation,
            IsDone = task.IsDone,
            UpdatedAt = task.UpdatedAt,
            LastRemindedAt = task.LastRemindedAt,
        };
    }
}
