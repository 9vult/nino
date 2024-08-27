using Newtonsoft.Json;
using Nino.Records.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Project
    {
        public required string Id;
        [JsonIgnore] public required ulong GuildId;
        public required string Nickname;
        public required string Title;
        [JsonIgnore] public required ulong OwnerId;
        [JsonIgnore] public required ulong[] AdministratorIds;
        public required Staff[] KeyStaff;
        public required ProjectType Type;
        public required string PosterUri;
        [JsonIgnore] public required ulong UpdateChannelId;
        [JsonIgnore] public required ulong ReleaseChannelId;
        public required bool IsPrivate;
        public required CongaParticipant[] CongaParticipants;
        public required string[] Aliases;
        public string? Motd;
        public string? AniDBId;
        public string? AirTime;
        public required bool AirReminderEnabled;
        [JsonIgnore] public ulong? AirReminderChannelId;
        [JsonIgnore] public ulong? AirReminderRoleId;

        //
        // Serialization stuff because azure doesn't support ulong
        //

        [JsonProperty("GuildId")]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }

        [JsonProperty("OwnerId")]
        public string SerializationOwnerId
        {
            get => OwnerId.ToString();
            set => OwnerId = ulong.Parse(value);
        }

        [JsonProperty("AdministratorIds")]
        public string[] SerializationAdministratorIds
        {
            get => AdministratorIds != null && AdministratorIds.Length != 0 ? AdministratorIds.Select(a => a.ToString()).ToArray() : [];
            set => AdministratorIds = value != null && value.Length != 0 ? value.Select(ulong.Parse).ToArray() : [];
        }

        [JsonProperty("UpdateChannelId")]
        public string SerializationUpdateChannelId
        {
            get => UpdateChannelId.ToString();
            set => UpdateChannelId = ulong.Parse(value);
        }

        [JsonProperty("ReleaseChannelId")]
        public string SerializationReleaseChannelId
        {
            get => ReleaseChannelId.ToString();
            set => ReleaseChannelId = ulong.Parse(value);
        }

        [JsonProperty("AirReminderChannelId")]
        public string? SerializationAirReminderChannelId
        {
            get => AirReminderChannelId?.ToString();
            set => AirReminderChannelId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }

        [JsonProperty("AirReminderRoleId")]
        public string? SerializationAirReminderRoleId
        {
            get => AirReminderRoleId?.ToString();
            set => AirReminderRoleId = !string.IsNullOrEmpty(value) ? ulong.Parse(value) : null;
        }
    }
}
