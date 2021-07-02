using Core.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Services.Test.IntegrationTests
{
    public static class ConfigurationUtility
    {
        public static IOptions<DatabaseConfiguration> GetDatabaseConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();
            var option = config.GetSection(DatabaseConfiguration.Key).Get<DatabaseConfiguration>();

            return Options.Create(option);
        }
    }
}
