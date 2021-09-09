using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public class CurrentAspNetRequest : ICurrentHttpRequest
    {
        private readonly ILogger _logger;

        public CurrentAspNetRequest(ILogger logger)
        {
            _logger = logger;
        }

        public ISet<string> GetDefinedJsonRootProperties()
        {
            var requestInputStream = HttpContext.Current.Request.InputStream;
            try
            {
                requestInputStream.Position = 0;
                using var jsonTextReader = new JsonTextReader(new StreamReader(requestInputStream));
                var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var root = JObject.ReadFrom(jsonTextReader);
                foreach (var token in root.Children())
                {
                    properties.Add(token.Path);
                }

                return properties;
            }
            catch (Exception e)
            {
                _logger.ForContext<CurrentAspNetRequest>().Error(e, "Failed while inpecting root properties");
                return new HashSet<string>();
            }
            finally
            {
                requestInputStream.Position = 0;
            }
        }
    }
}