using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class ObserverMapper
{
    public static ObserverDto ToDto(this Observer domain)
    {
        return new ObserverDto
        {
            Id = domain.Id,
            GuildId = domain.GuildId.ToString(),
            OriginGuildId = domain.OriginGuildId.ToString(),
            OwnerId = domain.OwnerId.ToString(),
            ProjectId = domain.ProjectId,
            Blame = domain.Blame,
            RoleId = domain.RoleId?.ToString(),
            ProgressWebhook = domain.ProgressWebhook,
            ReleasesWebhook = domain.ReleasesWebhook,
        };
    }

    public static Observer FromDto(this ObserverDto dto)
    {
        return new Observer
        {
            Id = dto.Id,
            GuildId = ulong.Parse(dto.GuildId),
            OriginGuildId = ulong.Parse(dto.OriginGuildId),
            OwnerId = ulong.Parse(dto.OwnerId),
            ProjectId = dto.ProjectId,
            Blame = dto.Blame,
            RoleId = dto.RoleId is not null ? ulong.Parse(dto.RoleId) : null,
            ProgressWebhook = dto.ProgressWebhook,
            ReleasesWebhook = dto.ReleasesWebhook,
        };
    }
}