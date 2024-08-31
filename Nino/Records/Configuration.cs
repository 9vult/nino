using Newtonsoft.Json;
using Nino.Records.Enums;

namespace Nino.Records
{
    public record Configuration
    {
        public required string Id;
        [JsonIgnore] public required ulong GuildId;
        public required DisplayType UpdateDisplay;
        public required DisplayType ProgressDisplay;
        [JsonIgnore] public required ulong[] AdministratorIds;
        public string? ReleasePrefix;

        [JsonProperty("GuildId")]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }

        [JsonProperty("AdministratorIds")]
        public string[] SerializationAdministratorIds
        {
            get => AdministratorIds != null && AdministratorIds.Length != 0 ? AdministratorIds.Select(a => a.ToString()).ToArray() : [];
            set => AdministratorIds = value != null && value.Length != 0 ? value.Select(ulong.Parse).ToArray() : [];
        }

        /// <summary>
        /// Create a default configuration for a guild
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <returns>Default configuration options</returns>
        public static Configuration CreateDefault(ulong guildId) =>
            new()
            {
                Id = $"{guildId}-conf",
                GuildId = guildId,
                UpdateDisplay = DisplayType.Normal,
                ProgressDisplay = DisplayType.Succinct,
                AdministratorIds = [],
                ReleasePrefix = null
            };
    }
}
