using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.Description;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class RemoveInternalApiOperationsFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var apiDescription in apiExplorer.ApiDescriptions)
            {
                if (IsActionInternal(apiDescription))
                {
                    var route = "/" + apiDescription.RelativePath.TrimEnd('/');
                    swaggerDoc.paths.Remove(route);
                }
                if (IsControllerInternal(apiDescription))
                {
                    var route = Regex.Replace("/" + apiDescription.RelativePath.TrimEnd('/'), @"(\?.*)", "");
                    swaggerDoc.paths.Remove(route);
                }
            }
        }

        private static bool IsActionInternal(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.GetCustomAttributes<InternalApiAttribute>().Any();
        }

        private static bool IsControllerInternal(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<InternalApiAttribute>().Any();
        }
    }
}