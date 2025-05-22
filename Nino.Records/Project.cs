using Nino.Records.Enums;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using NJsonPropertyName = Newtonsoft.Json.JsonPropertyAttribute;
using STJsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using STJsonPropertyName = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Nino.Records
{
    public record Project
    {
        public required Guid Id;
        [NJsonIgnore] public required ulong GuildId;
        public required string Nickname;
        public required string Title;
        [NJsonIgnore] public required ulong OwnerId;
        [NJsonIgnore] public required ulong[] AdministratorIds;
        public required Staff[] KeyStaff;
        public required ProjectType Type;
        public required string PosterUri;
        [NJsonIgnore] public required ulong UpdateChannelId;
        [NJsonIgnore] public required ulong ReleaseChannelId;
        public required bool IsPrivate;
        public required bool IsArchived = false;
        [NJsonIgnore, STJsonIgnore] public required CongaGraph CongaParticipants = new();
        public required string[] Aliases;
        public string? Motd;
        public int? AniListId;
        public int? AniListOffset;
        public required bool AirReminderEnabled;
        [NJsonIgnore] public ulong? AirReminderChannelId;
        [NJsonIgnore] public ulong? AirReminderRoleId;
        [NJsonIgnore] public ulong? AirReminderUserId;
        public required bool CongaReminderEnabled;
        public TimeSpan? CongaReminderPeriod;
        [NJsonIgnore] public ulong? CongaReminderChannelId;
        public DateTimeOffset? Created;

        //
        // Serialization stuff because azure doesn't support ulong
        //

        [STJsonIgnore]
        [NJsonPropertyName("GuildId")]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }

        [STJsonIgnore]
        [NJsonPropertyName("OwnerId")]
        public string SerializationOwnerId
        {
            get => OwnerId.ToString();
            set => OwnerId = ulong.Parse(value);
        }

        [STJsonIgnore]
        [NJsonPropertyName("AdministratorIds")]
        public string[] SerializationAdministratorIds
        {
            get => AdministratorIds != null && AdministratorIds.Length != 0 ? AdministratorIds.Select(a => a.ToString()).ToArray() : [];
            set => AdministratorIds = value != null && value.Length != 0 ? value.Select(ulong.Parse).ToArray() : [];
        }

        [STJsonIgnore]
        [NJsonPropertyName("UpdateChannelId")]
        public string SerializationUpdateChannelId
        {
            get => UpdateChannelId.ToString();
            set => UpdateChannelId = ulong.Parse(value);
        }

        [STJsonIgnore]
        [NJsonPropertyName("ReleaseChannelId")]
        public string SerializationReleaseChannelId
        {
            get => ReleaseChannelId.ToString();
            set => ReleaseChannelId = ulong.Parse(value);
        }

        [STJsonIgnore]
        [NJsonPropertyName("AirReminderChannelId")]
        public string? SerializationAirReminderChannelId
        {
            get => AirReminderChannelId?.ToString();
            set => AirReminderChannelId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }

        [STJsonIgnore]
        [NJsonPropertyName("AirReminderRoleId")]
        public string? SerializationAirReminderRoleId
        {
            get => AirReminderRoleId?.ToString();
            set => AirReminderRoleId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }
        
        [STJsonIgnore]
        [NJsonPropertyName("AirReminderUserId")]
        public string? SerializationAirReminderUserId
        {
            get => AirReminderUserId?.ToString();
            set => AirReminderUserId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }
        
        [STJsonIgnore]
        [NJsonPropertyName("CongaReminderChannelId")]
        public string? SerializationCongaReminderChannelId
        {
            get => CongaReminderChannelId?.ToString();
            set => CongaReminderChannelId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }
        
        /// <summary>
        /// AniList URL. <see langword="null"/> if <see cref="AniListId"/> is <see langword="null"/>.
        /// </summary>
        [STJsonIgnore]
        public string? AniListUrl
        {
            get => AniListId is null ? null : $"https://anilist.co/anime/{AniListId}";
        }
        
        [STJsonPropertyName("CongaParticipants")]
        [NJsonPropertyName("CongaParticipants")]
        public CongaNodeDto[] SerializationCongaGraph
        {
            get => CongaParticipants.Serialize().ToArray();
            set => CongaParticipants = CongaGraph.Deserialize(value);
        }

        public override string ToString ()
        {
            return $"P[{Id} ({GuildId}-{Nickname})]";
        }
    }
}
