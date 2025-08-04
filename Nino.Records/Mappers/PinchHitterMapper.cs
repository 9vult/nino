using Nino.Records.Dtos;

namespace Nino.Records.Mappers;

public static class PinchHitterMapper
{
    public static PinchHitterDto ToDto(this PinchHitter domain)
    {
        return new PinchHitterDto
        {
            Abbreviation = domain.Abbreviation,
            UserId = domain.UserId.ToString(),
        };
    }

    public static PinchHitter FromDto(this PinchHitterDto dto)
    {
        return new PinchHitter
        {
            Abbreviation = dto.Abbreviation,
            UserId = ulong.Parse(dto.UserId),
        };
    }
}