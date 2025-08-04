using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class ProjectMapper
{
    public static ProjectDto ToDto(this Project domain)
    {
        return new ProjectDto
        {
            Id = domain.Id,
            GuildId = domain.GuildId.ToString(),
            Nickname = domain.Nickname,
            Title = domain.Title,
            OwnerId = domain.OwnerId.ToString(),
            AdministratorIds = domain.AdministratorIds.Select(id => id.ToString()).ToList(),
            KeyStaff = domain.KeyStaff.Select(StaffMapper.ToDto).ToList(),
            Type = domain.Type,
            PosterUri = domain.PosterUri,
            UpdateChannelId = domain.UpdateChannelId.ToString(),
            ReleaseChannelId = domain.ReleaseChannelId.ToString(),
            IsPrivate = domain.IsPrivate,
            IsArchived = domain.IsArchived,
            CongaParticipants = domain.CongaParticipants.ToDto(),
            Aliases = domain.Aliases.ToList(),
            Motd = domain.Motd,
            AniListId = domain.AniListId,
            AniListOffset = domain.AniListOffset,
            AirReminderEnabled = domain.AirReminderEnabled,
            AirReminderChannelId = domain.AirReminderChannelId?.ToString(),
            AirReminderRoleId = domain.AirReminderRoleId?.ToString(),
            AirReminderUserId = domain.AirReminderUserId?.ToString(),
            CongaReminderEnabled = domain.CongaReminderEnabled,
            CongaReminderPeriod = domain.CongaReminderPeriod,
            CongaReminderChannelId = domain.CongaReminderChannelId?.ToString(),
            Created = domain.Created,
        };
    }

    public static Project FromDto(this ProjectDto dto)
    {
        return new Project
        {
            Id = dto.Id,
            GuildId = ulong.Parse(dto.GuildId),
            Nickname = dto.Nickname,
            Title = dto.Title,
            OwnerId = ulong.Parse(dto.OwnerId),
            AdministratorIds = dto.AdministratorIds.Select(ulong.Parse).ToArray(),
            KeyStaff = dto.KeyStaff.Select(StaffMapper.FromDto).ToArray(),
            Type = dto.Type,
            PosterUri = dto.PosterUri,
            UpdateChannelId = ulong.Parse(dto.UpdateChannelId),
            ReleaseChannelId = ulong.Parse(dto.ReleaseChannelId),
            IsPrivate = dto.IsPrivate,
            IsArchived = dto.IsArchived,
            CongaParticipants = dto.CongaParticipants.FromDto(),
            Aliases = dto.Aliases.ToArray(),
            Motd = dto.Motd,
            AniListId = dto.AniListId,
            AniListOffset = dto.AniListOffset,
            AirReminderEnabled = dto.AirReminderEnabled,
            AirReminderChannelId = dto.AirReminderChannelId is not null ? ulong.Parse(dto.AirReminderChannelId) : null,
            AirReminderRoleId = dto.AirReminderRoleId is not null ? ulong.Parse(dto.AirReminderRoleId) : null,
            AirReminderUserId = dto.AirReminderUserId is not null ? ulong.Parse(dto.AirReminderUserId) : null,
            CongaReminderEnabled = dto.CongaReminderEnabled,
            CongaReminderPeriod = dto.CongaReminderPeriod,
            CongaReminderChannelId = dto.CongaReminderChannelId is not null ? ulong.Parse(dto.CongaReminderChannelId) : null,
            Created = dto.Created,
        };
    }
}