namespace Nino.Utilities.AzureDtos;

public record StaffDto
{
    public required string UserId { get; set; }
    public required RoleDto Role { get; set; }
    public bool? IsPseudo { get; set; }
}