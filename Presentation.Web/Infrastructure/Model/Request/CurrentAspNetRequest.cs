using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Abstractions.Extensions;
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

        public ISet<string> GetDefinedJsonProperties(IEnumerable<string> pathTokens)
        {
            var requestInputStream = _currentRequestStream.GetInputStreamCopy();
            try
            {
                using var jsonTextReader = new JsonTextReader(new StreamReader(requestInputStream));
                var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var rootResult = TraverseTo(JObject.ReadFrom(jsonTextReader), pathTokens);

                var propertyNames = rootResult
                    .Select(rootToken => rootToken.Children().Select(ExtractPropertyName))
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
        }

        public Maybe<JToken> GetObject(IEnumerable<string> pathTokens)
        {
            var requestInputStream = _currentRequestStream.GetInputStreamCopy();
            try
            {
                using var jsonTextReader = new JsonTextReader(new StreamReader(requestInputStream));

                return TraverseTo(JObject.ReadFrom(jsonTextReader), pathTokens);
                
            }
            catch (Exception e)
            {
                _logger.ForContext<CurrentAspNetRequest>().Error(e, "Failed while inspecting root properties");
                return Maybe<JToken>.None;
            }
        }

        private static string ExtractPropertyName(JToken child)
        {
            return child.Path.Substring(child.Parent?.Path?.Length ?? 0, child.Path.Length - child.Parent?.Path?.Length ?? 0).TrimStart('.');
        }

        private static Maybe<JToken> TraverseTo(JToken root, IEnumerable<string> pathTokens)
        {
            var currentRoot = root;
            var tokensToDescendInto = new Stack<string>(pathTokens.Reverse());

            while (currentRoot != null && tokensToDescendInto.Any())
            {
                var propertyName = tokensToDescendInto.Pop();
                currentRoot = currentRoot.Children().FirstOrDefault(x => x.Transform(ExtractPropertyName).Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
            }

            return tokensToDescendInto.Any() ? Maybe<JToken>.None : currentRoot;
        }
    }
}