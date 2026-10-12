using System;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class PurgeUnusedTypesDocumentFilter : IDocumentFilter
    {
        private readonly Predicate<OpenApiDocument> _applyTo;

        public PurgeUnusedTypesDocumentFilter(Predicate<OpenApiDocument> applyTo)
        {
            _applyTo = applyTo;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (!_applyTo(swaggerDoc)) return;

            if (swaggerDoc.Components?.Schemas == null) return;

            var referencedSchemaKeys = swaggerDoc.Paths
                .SelectMany(p => p.Value.EnumerateOperations())
                .SelectMany(op => op.EnumerateSchemas())
                .SelectMany(s => s.StartSchemaEnumeration(swaggerDoc.Components.Schemas))
                .Where(s => s.GetRootSchemaOrNull() != null)
                .Select(s => s.GetSchemaTypeKey())
                .Where(k => k != null)
                .Distinct()
                .ToHashSet();

            var unreferenced = swaggerDoc.Components.Schemas
                .Where(x => !referencedSchemaKeys.Contains(x.Key))
                .Select(x => x.Key)
                .ToList();

            foreach (var key in unreferenced)
                swaggerDoc.Components.Schemas.Remove(key);
        }
    }
}
