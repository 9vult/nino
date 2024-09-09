using Newtonsoft.Json;

namespace Nino.Records
{
    public record Staff
    {
        [JsonIgnore] public required ulong UserId;
        public required Role Role;

        [JsonProperty("UserId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SerializationUserId
        {
            get => UserId.ToString();
            set => UserId = ulong.Parse(value);
        }
    }
}
