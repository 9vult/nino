using Nino.Records.Enums;

namespace Nino.Records.Json;

public class ProjectCreateDto
{
    public required string Nickname;
    public required string Title;
    public required ProjectType Type;
    public required uint Length;
    public required bool IsPrivate;
    public required ulong UpdateChannelId;
    public required ulong ReleaseChannelId;
    public int AniListId;
    
    public required string? PosterUri;
    public decimal? FirstEpisode;
    public ulong[]? AdministratorIds;
    public string[]? Aliases;
    public CongaNodeDto[]? CongaParticipants;
    
    public required StaffCreateDto[] KeyStaff;
    public required Dictionary<string, StaffCreateDto[]> AdditionalStaff;
}