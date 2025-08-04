using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class EpisodeMapper
{
    public static EpisodeDto ToDto(this Episode domain)
    {
        return new EpisodeDto
        {
            Id = domain.Id,
            ProjectId = domain.ProjectId,
            GuildId = domain.GuildId.ToString(),
            Number = domain.Number,
            Done = domain.Done,
            ReminderPosted = domain.ReminderPosted,
            AdditionalStaff = domain.AdditionalStaff.Select(StaffMapper.ToDto).ToList(),
            PinchHitters = domain.PinchHitters.Select(PinchHitterMapper.ToDto).ToList(),
            Tasks = domain.Tasks.Select(TaskMapper.ToDto).ToList(),
            Updated = domain.Updated,
        };
    }

    public static Episode FromDto(this EpisodeDto dto)
    {
        return new Episode
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            GuildId = ulong.Parse(dto.GuildId),
            Number = dto.Number,
            Done = dto.Done,
            ReminderPosted = dto.ReminderPosted,
            AdditionalStaff = dto.AdditionalStaff.Select(StaffMapper.FromDto).ToArray(),
            PinchHitters = dto.PinchHitters.Select(PinchHitterMapper.FromDto).ToArray(),
            Tasks = dto.Tasks.Select(TaskMapper.FromDto).ToArray(),
            Updated = dto.Updated,
        };
    }
}