using Newtonsoft.Json;

namespace Xero.Api.Migrate.Core.Library.Models
{
    public class OAuth2TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("xero_tenant_id")]
        public string XeroTenantId { get; set; }
    }
}