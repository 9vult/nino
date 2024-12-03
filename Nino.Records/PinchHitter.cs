using Newtonsoft.Json;

namespace Nino.Records
{
    public record PinchHitter
    {
        [JsonIgnore] public required ulong UserId;
        public required string Abbreviation;

        [JsonProperty("UserId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SerializationUserId
        {
            get => UserId.ToString();
            set => UserId = ulong.Parse(value);
        }
    }
}
