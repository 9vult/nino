using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class TaskMapper
{
    public static TaskDto ToDto(this Task domain)
    {
        return new TaskDto
        {
            Abbreviation = domain.Abbreviation,
            Done = domain.Done,
            Updated = domain.Updated,
            LastReminded = domain.LastReminded,
        };
    }

    public static Task FromDto(this TaskDto dto)
    {
        return new Task
        {
            Abbreviation = dto.Abbreviation,
            Done = dto.Done,
            Updated = dto.Updated,
            LastReminded = dto.LastReminded,
        };
    }
}