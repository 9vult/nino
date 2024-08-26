using Newtonsoft.Json;
using Nino.Records.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Configuration
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
    }
}
