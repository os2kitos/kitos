using System;
using System.Linq;
using Core.DomainModel;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class OnlyIncludeReadModelSchemasInSwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Simplified - no OData-specific filtering needed in ASP.NET Core version
        }
    }
}
