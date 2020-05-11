using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xero.Api.Migrate.Core.Library.Models;

namespace Xero.Api.Migrate.Core.Library
{
    public class TokenMigrator
    {
        private readonly XeroApiSettings _xeroApiSettings;

        private const string MigratePath = "/oauth/migrate";
        private const string AuthScheme = "OAuth";
        private const string MediaType = "application/json";

        private readonly X509Certificate2 _certificate;
        private readonly HttpClient _httpClient;

        public TokenMigrator(XeroApiSettings xeroApiSettings)
        {
            _xeroApiSettings = xeroApiSettings;

            _certificate = new X509Certificate2(xeroApiSettings.SigningCertificatePath, xeroApiSettings.SigningCertificatePassword, X509KeyStorageFlags.MachineKeySet);

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(xeroApiSettings.BaseUrl)
            };
        }

        public async Task<OAuth2TokenResponse> Migrate(string accessToken, string tenantType)
        {
            var requestBody = new OAuth2TokenRequest
            {
                ClientId = _xeroApiSettings.ClientId,
                ClientSecret = _xeroApiSettings.ClientSecret,
                Scope = _xeroApiSettings.Scope
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, MediaType);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{MigratePath}?tenantType={tenantType}")
            {
                Content = content
            };

            var authorizationHeaderValue = BuildOAuth1AuthorizationHeader(accessToken, tenantType);

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, authorizationHeaderValue);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token migration failed for access token '{accessToken}' with status code {response.StatusCode} - {responseBody}");
            }

            return JsonConvert.DeserializeObject<OAuth2TokenResponse>(responseBody);
        }

        private string BuildOAuth1AuthorizationHeader(string accessToken, string tenantType)
        {
            var nonce = Guid.NewGuid().ToString();

            var currentTimestamp = CurrentTimestamp();

            var baseSignatureString = GenerateBaseSignatureString(accessToken, tenantType, nonce, currentTimestamp);

            var signatureString = Sign(baseSignatureString);

            return GenerateAuthorizationHeader(accessToken, nonce, currentTimestamp, signatureString);
        }

        private string CurrentTimestamp()
        {
            var t = DateTime.UtcNow - new DateTime(1970, 1, 1);

            var secondsSinceEpoch = (int)t.TotalSeconds;

            return secondsSinceEpoch.ToString();
        }

        private string GenerateBaseSignatureString(string accessToken, string tenantType, string nonce, string currentTimestamp)
        {
            const string httpMethod = "POST";
            
            var url = $"{_xeroApiSettings.BaseUrl}{MigratePath}".Escape();

            var oauthParameterStringBuilder = new StringBuilder();

            oauthParameterStringBuilder.Append($"oauth_consumer_key={_xeroApiSettings.ConsumerKey}&");
            oauthParameterStringBuilder.Append($"oauth_nonce={nonce}&");
            oauthParameterStringBuilder.Append("oauth_signature_method=RSA-SHA1&");
            oauthParameterStringBuilder.Append($"oauth_timestamp={currentTimestamp}&");
            oauthParameterStringBuilder.Append($"oauth_token={accessToken}&");
            oauthParameterStringBuilder.Append("oauth_version=1.0&");
            oauthParameterStringBuilder.Append($"tenantType={tenantType}");

            var oauthParameterString = oauthParameterStringBuilder.ToString().Escape();

            return $"{httpMethod}&{url}&{oauthParameterString}";
        }

        private string Sign(string signatureBaseString)
        {
            using (var sha1 = SHA1.Create())
            {
                var bytes = Encoding.ASCII.GetBytes(signatureBaseString);

                var hash = sha1.ComputeHash(bytes);

                var sig = _certificate.GetRSAPrivateKey().SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

                return Convert.ToBase64String(sig);
            }
        }

        private string GenerateAuthorizationHeader(string accessToken, string nonce, string currentTimestamp, string encodedSignature)
        {
            var authorizationHeaderBuilder = new StringBuilder();

            authorizationHeaderBuilder.Append($"oauth_consumer_key=\"{_xeroApiSettings.ConsumerKey}\", ");
            authorizationHeaderBuilder.Append($"oauth_token=\"{accessToken}\", ");
            authorizationHeaderBuilder.Append("oauth_signature_method=\"RSA-SHA1\", ");
            authorizationHeaderBuilder.Append($"oauth_signature=\"{encodedSignature.Escape()}\", ");
            authorizationHeaderBuilder.Append($"oauth_timestamp=\"{currentTimestamp}\", ");
            authorizationHeaderBuilder.Append($"oauth_nonce=\"{nonce}\", ");
            authorizationHeaderBuilder.Append("oauth_version=\"1.0\"");

            return authorizationHeaderBuilder.ToString();
        }
    }
}
