
using Newtonsoft.Json;

namespace Nino.Records
{
    public record Observer
    {
        public required string Id;
        [JsonIgnore] public required ulong GuildId;
        [JsonIgnore] public required ulong OriginGuildId;
        [JsonIgnore] public required ulong OwnerId;
        public required string ProjectId;
        public required bool Blame;
        [JsonIgnore] public ulong? RoleId;
        public string? ProgressWebhook;
        public string? ReleasesWebhook;

        [JsonProperty("GuildId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }

        [JsonProperty("OriginGuildId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SerializationOriginGuildId
        {
            get => OriginGuildId.ToString();
            set => OriginGuildId = ulong.Parse(value);
        }

        [JsonProperty("OwnerId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SerializationOwnerId
        {
            get => OwnerId.ToString();
            set => OwnerId = ulong.Parse(value);
        }

        [JsonProperty("RoleId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string? SerializationRoleId
        {
            get => RoleId?.ToString();
            set => RoleId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }
    }
}
