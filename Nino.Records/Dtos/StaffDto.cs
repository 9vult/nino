namespace Nino.Records.Dtos;

public record StaffDto
{
    public required string UserId { get; set; }
    public required RoleDto Role { get; set; }
    public required bool IsPseudo { get; set; }
}