using System;
using System.Threading.Tasks;
using Xero.Api.Migrate.Core.Library;

namespace Xero.Api.Migrate.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var apiSettings = new XeroApiSettings();

            if (apiSettings.TenantType != "ORGANISATION" && apiSettings.TenantType != "PRACTICE")
            {
                Console.WriteLine("We don't recognise your TenantType. TenantType must be defined in 'appsettings.json' as either 'ORGANISATION' or 'PRACTICE'");
                Console.ReadLine();
            }
            else
            {
                await MigrateAccessToken(args, apiSettings);
            }
        }

        private static async Task MigrateAccessToken(string[] args, XeroApiSettings apiSettings)
        {
            var tokenMigrator = new TokenMigrator(apiSettings);

            try
            {
                var oauth2TokenResponse = await tokenMigrator.Migrate(args[0], apiSettings.TenantType);

                Console.WriteLine();
                Console.WriteLine($"Access token: {oauth2TokenResponse.AccessToken}");
                Console.WriteLine();
                Console.WriteLine($"Refresh token: {oauth2TokenResponse.RefreshToken}");
                Console.WriteLine();
                Console.WriteLine($"Tenant Id: {oauth2TokenResponse.XeroTenantId}");
                Console.WriteLine();
                Console.WriteLine($"Expires in: {oauth2TokenResponse.ExpiresIn} seconds");
                Console.WriteLine();

                Console.WriteLine("Press any key to get the tenants for which these tokens apply...");
                Console.ReadLine();

                var xeroApi = new XeroApi(apiSettings, oauth2TokenResponse.AccessToken);

                Console.WriteLine("These tokens apply to connections for the following tenants. You should replace any persisted tokens for these tenants with the tokens displayed above.");
                Console.WriteLine();

                var connections = await xeroApi.GetConnections();
                foreach (var connection in connections)
                {
                    Console.WriteLine(connection);
                }

                Console.WriteLine();

                Console.WriteLine("Press any key to use your new access token to make a request to the Xero API for the migrated access token...");
                Console.ReadLine();

                if (apiSettings.TenantType == "ORGANISATION")
                {
                    var organisation = await xeroApi.GetOrganisation(oauth2TokenResponse.XeroTenantId);

                    Console.WriteLine("Organisation retrieved successfully:");
                    Console.WriteLine();
                    Console.WriteLine($"Name: '{organisation.Name}'");
                    Console.WriteLine($"Status: {organisation.OrganisationStatus}");
                    Console.WriteLine($"Country Code: {organisation.CountryCode}");
                    Console.WriteLine($"Is Demo Company? {organisation.IsDemoCompany}");
                    Console.WriteLine();

                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                }
                else
                {
                    var clientCount = await xeroApi.GetClientCount(oauth2TokenResponse.XeroTenantId);

                    Console.WriteLine("Clients retrieved successfully:");
                    Console.WriteLine();
                    Console.WriteLine($"Total count: '{clientCount}'");
                    Console.WriteLine();

                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
