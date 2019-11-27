using Microsoft.Extensions.Configuration;

namespace Xero.Api.Migrate.Core.Library
{
    public class XeroApiSettings
    {
        public IConfigurationSection ApiSettings { get; set; }

        public XeroApiSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            ApiSettings = builder.GetSection("XeroApi");
        }

        public string BaseUrl => ApiSettings["BaseUrl"];

        public string ConsumerKey => ApiSettings["ConsumerKey"];
        
        public string ClientId => ApiSettings["ClientId"];
        
        public string ClientSecret => ApiSettings["ClientSecret"];
        
        public string Scope => ApiSettings["Scope"];

        public string SigningCertificatePath => ApiSettings["SigningCertPath"];

        public string SigningCertificatePassword => ApiSettings["SigningCertPassword"];
    }
}
