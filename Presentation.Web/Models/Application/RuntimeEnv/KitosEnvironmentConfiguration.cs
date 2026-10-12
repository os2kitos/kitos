using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Presentation.Web.Models.Application.RuntimeEnv
{
    public class KitosEnvironmentConfiguration
    {
        public KitosEnvironment Environment { get; }

        public KitosEnvironmentConfiguration(KitosEnvironment environment)
        {
            Environment = environment;
        }

        public static KitosEnvironmentConfiguration FromConfiguration(IConfiguration configuration)
        {
            var environmentConf = configuration["AppSettings:Environment"]?.ToLowerInvariant() ?? "";
            var env = environmentConf switch
            {
                "dev" => KitosEnvironment.Dev,
                "integration" => KitosEnvironment.Integration,
                "staging" => KitosEnvironment.Staging,
                "prod" => KitosEnvironment.Production,
                _ => throw new ConfigurationErrorsException(
                    $"Invalid value of the Environment variable. Got:\"{environmentConf}\"")
            };

            return new KitosEnvironmentConfiguration(env);
        }
    }
}