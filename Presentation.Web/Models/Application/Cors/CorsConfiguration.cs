using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http.Cors;
using Core.Abstractions.Types;
using Presentation.Web.Models.Application.RuntimeEnv;
using Presentation.Web.Properties;

namespace Presentation.Web.Models.Application.Cors
{
    public class CorsConfiguration
    {
        private const string WildCard = "*";
        public Maybe<EnableCorsAttribute> GlobalCorsSettings { get; }

        public static CorsConfiguration FromConfiguration()
        {
            var environmentConfiguration = KitosEnvironmentConfiguration.FromConfiguration();
            var config = Maybe<EnableCorsAttribute>.None;

            if (environmentConfiguration.Environment == KitosEnvironment.Dev)
            {
                var origins = Settings.Default.CorsOrigins;
                if (!string.IsNullOrWhiteSpace(origins))
                {
                    var configuredOrigins = origins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    var originsString = string.Join(",", configuredOrigins);
                    Trace.WriteLine($"CORS origins enabled:{originsString}");
                    if (originsString.Length > 0)
                    {
                        config = new EnableCorsAttribute(originsString, WildCard, WildCard);
                    }
                }
            }

            return new CorsConfiguration(config);
        }

        public CorsConfiguration(Maybe<EnableCorsAttribute> globalCorsSettings)
        {
            GlobalCorsSettings = globalCorsSettings;
        }
    }
}