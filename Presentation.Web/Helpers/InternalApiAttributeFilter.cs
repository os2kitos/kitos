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
                if(!apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<InternalApiAttribute>().Any() && !apiDescription.ActionDescriptor.GetCustomAttributes<InternalApiAttribute>().Any()) continue;
                var route = "/" + apiDescription.RelativePath.TrimEnd('/');
                swaggerDoc.paths.Remove(route);
            }
        }
    }
}