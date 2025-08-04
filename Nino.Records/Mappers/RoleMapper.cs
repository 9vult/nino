using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(this Role domain)
    {
        return new RoleDto
        {
            Abbreviation = domain.Abbreviation,
            Name = domain.Name,
            Weight = domain.Weight,
        };
    }

    public static Role FromDto(this RoleDto dto)
    {
        return new Role
        {
            Abbreviation = dto.Abbreviation,
            Name = dto.Name,
            Weight = dto.Weight,
        };
    }
}