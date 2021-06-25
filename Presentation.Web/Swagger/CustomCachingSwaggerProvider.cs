using System.Collections.Concurrent;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class CustomCachingSwaggerProvider : ISwaggerProvider
    {
            private static readonly ConcurrentDictionary<string, SwaggerDocument> Cache = new();

            private readonly ISwaggerProvider _swaggerProvider;

            public CustomCachingSwaggerProvider(ISwaggerProvider swaggerProvider)
            {
                _swaggerProvider = swaggerProvider;
            }

            public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
            {
                var cacheKey = $"{rootUrl}_{apiVersion}";
                return Cache.GetOrAdd(cacheKey, (key) => _swaggerProvider.GetSwagger(rootUrl, apiVersion));
            }
    }
}