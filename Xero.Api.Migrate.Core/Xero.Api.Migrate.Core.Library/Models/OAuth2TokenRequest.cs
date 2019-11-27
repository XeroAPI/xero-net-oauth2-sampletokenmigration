using Newtonsoft.Json;

namespace Xero.Api.Migrate.Core.Library.Models
{
    public class OAuth2TokenRequest
    {
        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }
}