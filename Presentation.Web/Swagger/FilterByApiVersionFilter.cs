using System;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class FilterByApiVersionFilter : IDocumentFilter
    {
        private readonly Func<OpenApiDocument, int> _getApiVersion;
        private readonly Func<string, int> _getPathApiVersion;

        public FilterByApiVersionFilter(Func<OpenApiDocument, int> getApiVersion, Func<string, int> getPathApiVersion)
        {
            _getApiVersion = getApiVersion;
            _getPathApiVersion = getPathApiVersion;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var docVersion = _getApiVersion(swaggerDoc);
            foreach (var path in swaggerDoc.Paths.Where(path => docVersion != _getPathApiVersion(path.Key)).ToList())
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }
    }
}
