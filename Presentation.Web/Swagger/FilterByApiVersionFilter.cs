using System;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class FilterByApiVersionFilter : IDocumentFilter
    {
        private readonly Func<SwaggerDocument, int> _getApiVersion;
        private readonly Func<string, int> _getPathApiVersion;

        public FilterByApiVersionFilter(Func<SwaggerDocument, int> getApiVersion, Func<string, int> getPathApiVersion)
        {
            _getApiVersion = getApiVersion;
            _getPathApiVersion = getPathApiVersion;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var docVersion = _getApiVersion(swaggerDoc);
            foreach (var path in swaggerDoc.paths.ToList())
            {
                if (docVersion != _getPathApiVersion(path.Key))
                {
                    swaggerDoc.paths.Remove(path);
                }
            }
        }
    }
}