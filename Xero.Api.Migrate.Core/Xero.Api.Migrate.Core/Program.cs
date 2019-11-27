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

            var tokenMigrator = new TokenMigrator(apiSettings);
            
            try
            {
                var oauth2TokenResponse = await tokenMigrator.Migrate(args[0]);

                Console.WriteLine();
                Console.WriteLine($"Access token: {oauth2TokenResponse.AccessToken}");
                Console.WriteLine();
                Console.WriteLine($"Refresh token: {oauth2TokenResponse.RefreshToken}");
                Console.WriteLine();
                Console.WriteLine($"Organisation Id: {oauth2TokenResponse.XeroTenantId}");
                Console.WriteLine();
                Console.WriteLine($"Expires in: {oauth2TokenResponse.ExpiresIn} seconds");
                Console.WriteLine();

                Console.WriteLine("Press any key to get the organisations for which these tokens apply...");
                Console.ReadLine();

                var xeroApi = new XeroApi(apiSettings, oauth2TokenResponse.AccessToken);

                Console.WriteLine("These tokens apply to connections for the following organisations. You should replace any persisted tokens for these organisations with the tokens displayed above.");
                Console.WriteLine();

                var organisationConnections = await xeroApi.GetConnections();
                foreach (var organisationConnection in organisationConnections)
                {
                    Console.WriteLine(organisationConnection);
                }
                Console.WriteLine();

                Console.WriteLine("Press any key to use your new access token to retrieve the organisation from the Xero API...");
                Console.ReadLine();

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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
