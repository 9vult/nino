using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class StaffMapper
{
    public static StaffDto ToDto(this Staff domain)
    {
        return new StaffDto
        {
            UserId = domain.UserId.ToString(),
            Role = domain.Role.ToDto(),
            IsPseudo = domain.IsPseudo,
        };
    }

    public static Staff FromDto(this StaffDto dto)
    {
        return new Staff
        {
            UserId = ulong.Parse(dto.UserId),
            Role = dto.Role.FromDto(),
            IsPseudo = dto.IsPseudo,
        };
    }
}