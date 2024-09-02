﻿using Newtonsoft.Json;

namespace Nino.Records
{
    public record Episode
    {
        public required string Id;
        public required string ProjectId;
        [JsonIgnore] public required ulong GuildId;
        public required decimal Number;
        public required bool Done;
        public required bool ReminderPosted;
        public required Staff[] AdditionalStaff;
        public required Task[] Tasks;
        public DateTime? Updated;

        [JsonProperty("GuildId")]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }
    }
}