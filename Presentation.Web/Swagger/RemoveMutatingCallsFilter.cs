using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class RemoveMutatingCallsFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var swaggerDocPath in swaggerDoc.paths)
            {
                var pathItem = swaggerDocPath.Value;
                pathItem.delete = null;
                pathItem.post = null;
                pathItem.patch = null;
                pathItem.put = null;
            }
        }
    }
}