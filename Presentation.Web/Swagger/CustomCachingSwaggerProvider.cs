using System.Collections.Concurrent;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace Presentation.Web.Swagger
{
    public class CustomCachingSwaggerProvider : ISwaggerProvider
    {
        private static readonly ConcurrentDictionary<string, OpenApiDocument> Cache = new();
        private readonly ISwaggerProvider _swaggerProvider;

        public CustomCachingSwaggerProvider(ISwaggerProvider swaggerProvider)
        {
            _swaggerProvider = swaggerProvider;
        }

        public OpenApiDocument GetSwagger(string documentName, string host, string basePath)
        {
            var cacheKey = $"{documentName}_{host}_{basePath}";
            return Cache.GetOrAdd(cacheKey, _ => _swaggerProvider.GetSwagger(documentName, host, basePath));
        }
    }
}
