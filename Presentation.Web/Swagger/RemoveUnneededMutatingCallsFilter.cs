using System;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class RemoveUnneededMutatingCallsFilter : IDocumentFilter
    {
        private readonly Predicate<OpenApiDocument> _applyTo;

        public RemoveUnneededMutatingCallsFilter(Predicate<OpenApiDocument> applyTo)
        {
            _applyTo = applyTo;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (!_applyTo(swaggerDoc)) return;

            foreach (var path in swaggerDoc.Paths.ToList())
            {
                if (IsExternalEndpointDocs(path.Key) || IsNeeded(path.Key))
                    continue;
                NukeWriteOperationDocs(path.Value);
            }
        }

        private static bool IsNeeded(string path) =>
            path.ToLowerInvariant().Contains("api/authorize") ||
            path.ToLowerInvariant().Contains("api/passwordresetrequest");

        private static bool IsExternalEndpointDocs(string path) =>
            path.Contains("/api/v2");

        private static void NukeWriteOperationDocs(IOpenApiPathItem pathItem)
        {
            if (pathItem.Operations == null)
                return;

            pathItem.Operations.Remove(System.Net.Http.HttpMethod.Delete);
            pathItem.Operations.Remove(System.Net.Http.HttpMethod.Post);
            pathItem.Operations.Remove(System.Net.Http.HttpMethod.Patch);
            pathItem.Operations.Remove(System.Net.Http.HttpMethod.Put);
        }
    }
}
