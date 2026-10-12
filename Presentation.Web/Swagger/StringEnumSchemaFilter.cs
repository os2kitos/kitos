using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Presentation.Web.Swagger
{
    public class StringEnumSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum) return;
            if (schema is not OpenApiSchema mutableSchema) return;

            mutableSchema.Type = JsonSchemaType.String;
            mutableSchema.Format = null;
            mutableSchema.Enum?.Clear();
            mutableSchema.Enum ??= new List<JsonNode>();
            foreach (var name in Enum.GetNames(context.Type))
            {
                mutableSchema.Enum.Add(JsonValue.Create(name));
            }
        }
    }
}
