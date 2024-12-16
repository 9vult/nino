using Newtonsoft.Json;

namespace Nino.Records
{
    public record Episode
    {
        public required Guid Id;
        public required Guid ProjectId;
        [JsonIgnore] public required ulong GuildId;
        public required string Number;
        public required bool Done;
        public required bool ReminderPosted;
        public required Staff[] AdditionalStaff;
        public required PinchHitter[] PinchHitters;
        public required Task[] Tasks;
        public DateTimeOffset? Updated;

        [JsonProperty("GuildId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SerializationGuildId
        {
            get => GuildId.ToString();
            set => GuildId = ulong.Parse(value);
        }

        public override string ToString ()
        {
            return $"E[{Id} ({Number})]";
        }
    }
}
