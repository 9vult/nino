
using Newtonsoft.Json;

namespace Nino.Records
{
    public record Observer
    {
        public required string Id;
        [JsonIgnore] public required ulong GuildId;
        [JsonIgnore] public required ulong OriginGuildId;
        public required string ProjectId;
        public required bool Blame;
        [JsonIgnore] public ulong? RoleId;
        public string? ProgressWebhook;
        public string? ReleasesWebhook;

        [JsonProperty("GuildId")]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }

        [JsonProperty("OriginGuildId")]
        public string SerializationOriginGuildId
        {
            get => OriginGuildId.ToString();
            set => OriginGuildId = ulong.Parse(value);
        }

        [JsonProperty("RoleId")]
        public string? SerializationRoleId
        {
            get => RoleId?.ToString();
            set => RoleId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }
    }
}
