using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Abstractions.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public class CurrentAspNetRequest : ICurrentHttpRequest
    {
        private readonly ILogger _logger;
        private readonly ICurrentRequestStream _currentRequestStream;

        public CurrentAspNetRequest(ILogger logger, ICurrentRequestStream currentRequestStream)
        {
            _logger = logger;
            _currentRequestStream = currentRequestStream;
        }

        public ISet<string> GetDefinedJsonProperties(params string[] pathTokens)
        {
            var requestInputStream = _currentRequestStream.GetCurrentInputStream();
            try
            {
                requestInputStream.Position = 0;
                using var jsonTextReader = new JsonTextReader(new StreamReader(requestInputStream));
                var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var rootResult = TraverseTo(JObject.ReadFrom(jsonTextReader), pathTokens);

                var propertyNames = rootResult
                    .Select(rootToken => rootToken.Children().Select(child => child.Path))
                    .Match(definedProperties => definedProperties, Array.Empty<string>);

                foreach (var propertyName in propertyNames)
                {
                    properties.Add(propertyName);
                }

                return properties;
            }
            catch (Exception e)
            {
                _logger.ForContext<CurrentAspNetRequest>().Error(e, "Failed while inspecting root properties");
                return new HashSet<string>();
            }
            finally
            {
                requestInputStream.Position = 0;
            }
        }

        private static Maybe<JToken> TraverseTo(JToken root, string[] pathTokens)
        {
            var currentRoot = root;
            var tokensToDescendInto = new Stack<string>(pathTokens.Reverse());

            while (currentRoot != null && tokensToDescendInto.Any())
            {
                var propertyName = tokensToDescendInto.Pop();
                currentRoot = currentRoot.Children().FirstOrDefault(x => x.Path.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            }

            return currentRoot;
        }
    }
}