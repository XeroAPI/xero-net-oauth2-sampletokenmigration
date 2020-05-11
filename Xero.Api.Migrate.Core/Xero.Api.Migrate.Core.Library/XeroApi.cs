using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xero.Api.Migrate.Core.Library.Models;

namespace Xero.Api.Migrate.Core.Library
{
    public class XeroApi
    {
        private const string AuthScheme = "Bearer";
        private const string XeroTenantIdHeader = "Xero-Tenant-Id";
        private const string OrganisationPath = "/api.xro/2.0/organisation";
        private const string ClientsPath = "/xero.hq/1.0/clients";
        private const string ConnectionsPath = "/connections";

        private readonly string _accessToken;
        private readonly HttpClient _httpClient;

        public XeroApi(XeroApiSettings xeroApiSettings, string accessToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(xeroApiSettings.BaseUrl)
            };

            _accessToken = accessToken;
        }

        public async Task<IEnumerable<string>> GetConnections()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ConnectionsPath);

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, _accessToken);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Connections retrieval failed for access token '{_accessToken}' with status code {response.StatusCode} - {responseBody}");
            }

            var connections = JsonConvert.DeserializeObject<List<ConnectionResponse>>(responseBody);

            return connections.Select(c => $"{c.TenantType}|{c.TenantId}");
        }

        public async Task<Organisation> GetOrganisation(string organisationId)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, OrganisationPath);

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, _accessToken);

            requestMessage.Headers.Add(XeroTenantIdHeader, organisationId);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Organisation retrieval failed for access token '{_accessToken}' and organisation id '{organisationId}' with status code {response.StatusCode} - {responseBody}");
            }

            var apiResponse = JsonConvert.DeserializeObject<OrganisationsResponse>(responseBody);

            return apiResponse.Organisations.FirstOrDefault();
        }

        public async Task<int> GetClientCount(string practiceId)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, ClientsPath);

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, _accessToken);

            requestMessage.Headers.Add(XeroTenantIdHeader, practiceId);

            var response = await _httpClient.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Practice client retrieval failed for access token '{_accessToken}' and practice id '{practiceId}' with status code {response.StatusCode} - {responseBody}");
            }

            var apiResponse = JsonConvert.DeserializeObject<ClientsResponse>(responseBody);

            return apiResponse.Pagination.Total;
        }
    }
}
