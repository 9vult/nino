using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class ConfigurationMapper
{
    public static ConfigurationDto ToDto(this Configuration domain)
    {
        return new ConfigurationDto
        {
            Id = domain.Id,
            GuildId = domain.GuildId.ToString(),
            UpdateDisplay = domain.UpdateDisplay,
            ProgressDisplay = domain.ProgressDisplay,
            CongaPrefix = domain.CongaPrefix,
            AdministratorIds = domain.AdministratorIds.Select(id => id.ToString()).ToList(),
            ReleasePrefix = domain.ReleasePrefix,
            Locale = domain.Locale,
        };
    }

    public static Configuration FromDto(this ConfigurationDto dto)
    {
        return new Configuration
        {
            Id = dto.Id,
            GuildId = ulong.Parse(dto.GuildId),
            UpdateDisplay = dto.UpdateDisplay,
            ProgressDisplay = dto.ProgressDisplay,
            CongaPrefix = dto.CongaPrefix,
            AdministratorIds = dto.AdministratorIds.Select(ulong.Parse).ToArray(),
            ReleasePrefix = dto.ReleasePrefix,
            Locale = dto.Locale,
        };
    }
}