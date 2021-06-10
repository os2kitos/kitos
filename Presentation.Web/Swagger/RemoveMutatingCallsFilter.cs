using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class RemoveMutatingCallsFilter : IDocumentFilter
    {
        private readonly Predicate<SwaggerDocument> _applyTo;

        public RemoveMutatingCallsFilter(Predicate<SwaggerDocument> applyTo)
        {
            _applyTo = applyTo;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            if (_applyTo(swaggerDoc))
            {
                foreach (var swaggerDocPath in swaggerDoc.paths)
                {
                    if (IsExternalEndpointDocs(swaggerDocPath.Key))
                        continue;

                    NukeWriteOperationDocs(swaggerDocPath);
                }
            }
        }

        private static bool IsExternalEndpointDocs(string path)
        {
            return path.Contains(@"/api/v2");
        }

        private static void NukeWriteOperationDocs(KeyValuePair<string, PathItem> swaggerDocPath)
        {
            var pathItem = swaggerDocPath.Value;
            pathItem.delete = null;
            pathItem.post = null;
            pathItem.patch = null;
            pathItem.put = null;
        }
    }
}