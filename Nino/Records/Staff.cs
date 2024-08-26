using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Staff
    {
        public required string Id;
        [JsonIgnore] public required ulong UserId;
        public required Role Role;

        [JsonProperty("UserId")]
        public string SerializationUserId
        {
            get => UserId.ToString();
            set => UserId = ulong.Parse(value);
        }
    }
}
