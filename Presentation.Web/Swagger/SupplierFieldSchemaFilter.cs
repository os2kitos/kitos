using System;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class SupplierFieldSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || context.Type == null) return;

            foreach (var property in context.Type.GetProperties())
            {
                var attr = property.GetCustomAttribute<SupplierFieldAttribute>();
                if (attr == null) continue;

                var schemaProp = schema.Properties
                    .FirstOrDefault(p => string.Equals(p.Key, property.Name, StringComparison.OrdinalIgnoreCase));

                if (schemaProp.Key != null && schemaProp.Value is OpenApiSchema mutableSchema)
                {
                    var description = mutableSchema.Description ?? string.Empty;
                    mutableSchema.Description = $"{description} (Supplier Field)";
                }
            }
        }
    }
}
