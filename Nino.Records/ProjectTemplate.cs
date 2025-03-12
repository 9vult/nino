using Nino.Records.Enums;

namespace Nino.Records;

public record ProjectTemplate
{
    public required string Nickname;
    public required string Title;
    public required ProjectType Type;
    public required uint Length;
    public required string PosterUri;
    public required bool IsPrivate;
    public required ulong UpdateChannelId;
    public required ulong ReleaseChannelId;
    
    public int? AniListId;
    public decimal? FirstEpisode;
    public ulong[]? AdministratorIds;
    public string[]? Aliases;
    public CongaParticipant[]? CongaParticipants;
    
    public required Staff[] KeyStaff;
    public required Dictionary<string, Staff[]> AdditionalStaff;
}