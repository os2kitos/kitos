using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Presentation.Web.Attributes;

namespace Presentation.Web.App_Start
{
    public class AddConsumesContentTypeOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            // Check if the action method has the SwaggerConsumesAttribute
            var consumesAttribute = apiDescription
                .ActionDescriptor
                .GetCustomAttributes<SwaggerConsumesAttribute>()
                .FirstOrDefault();

            string contentType = consumesAttribute?.ContentType ?? "application/json"; // Default to JSON if no attribute

            operation.consumes = new List<string> { contentType };
        }

    }

}