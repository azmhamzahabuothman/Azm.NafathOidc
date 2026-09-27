using Newtonsoft.Json;

namespace Azm.NafathOidc.Models
{
    public class NafathOidcJwkSet
    {
        [JsonProperty("keys")]
        public List<NafathOidcJwk> Keys { get; set; } = new();
    }

    public class NafathOidcJwk
    {
        [JsonProperty("kty")]
        public string Kty { get; set; } = default!;

        [JsonProperty("kid")]
        public string Kid { get; set; } = default!;

        [JsonProperty("use")]
        public string Use { get; set; } = default!;

        [JsonProperty("alg")]
        public string Alg { get; set; } = default!;

        [JsonProperty("n")]
        public string N { get; set; } = default!;

        [JsonProperty("e")]
        public string E { get; set; } = default!;
    }
}
