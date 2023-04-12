using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;
using System;
using System.Linq;

namespace Presentation.Web.Swagger
{
    public class RemoveUnneededMutatingCallsFilter : IDocumentFilter
    {
        private readonly Predicate<SwaggerDocument> _applyTo;

        public RemoveUnneededMutatingCallsFilter(Predicate<SwaggerDocument> applyTo)
        {
            _applyTo = applyTo;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            if (_applyTo(swaggerDoc))
            {
                foreach (var swaggerDocPath in swaggerDoc.paths.ToList())
                {
                    if (IsExternalEndpointDocs(swaggerDocPath.Key))
                        continue;
                    if (IsNeeded(swaggerDocPath.Key))
                        continue;
                    NukeWriteOperationDocs(swaggerDocPath);
                }
            }
        }

        private static bool IsNeeded(string path)
        {
            return path.ToLowerInvariant().Contains(@"api/authorize") || path.ToLowerInvariant().Contains(@"api/passwordresetrequest");
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