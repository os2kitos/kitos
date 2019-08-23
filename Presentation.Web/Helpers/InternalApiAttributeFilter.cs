using System.Linq;
using System.Web.Http.Description;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger;

namespace Presentation.Web.Helpers
{
    public class InternalApiAttributeFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var apiDescription in apiExplorer.ApiDescriptions)
            {
                var controllerIsInternal = apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<InternalApiAttribute>().Any();
                var actionIsInternal = apiDescription.ActionDescriptor.GetCustomAttributes<InternalApiAttribute>().Any();

                if (!controllerIsInternal && !actionIsInternal)
                {
                    continue;
                }
                var route = "/" + apiDescription.RelativePath.TrimEnd('/');
                swaggerDoc.paths.Remove(route);
            }
        }
    }
}