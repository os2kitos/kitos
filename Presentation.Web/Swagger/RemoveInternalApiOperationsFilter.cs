using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class RemoveInternalApiOperationsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                if (IsActionInternal(apiDescription))
                {
                    var route = "/" + apiDescription.RelativePath?.TrimEnd('/');
                    swaggerDoc.Paths.Remove(route);
                }
                else if (IsControllerInternal(apiDescription))
                {
                    var route = Regex.Replace("/" + apiDescription.RelativePath?.TrimEnd('/'), @"(\?.*)", "");
                    swaggerDoc.Paths.Remove(route);
                }
            }

            // Remove tag definitions that are no longer referenced by any remaining operation.
            // Without this, internal controllers leave behind empty groups in the Swagger UI.
            var usedTagNames = swaggerDoc.Paths
                .SelectMany(p => p.Value.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
                .SelectMany(op => op.Tags ?? new HashSet<OpenApiTagReference>())
                .Select(t => t.Name)
                .ToHashSet();

            var tagsToRemove = swaggerDoc.Tags?
                .Where(t => !usedTagNames.Contains(t.Name))
                .ToList();

            foreach (var tag in tagsToRemove ?? [])
                swaggerDoc.Tags?.Remove(tag);
        }

        private static bool IsActionInternal(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
        {
            if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                return actionDescriptor.MethodInfo.GetCustomAttributes(typeof(InternalApiAttribute), true).Any();
            return apiDescription.ActionDescriptor.EndpointMetadata.OfType<InternalApiAttribute>().Any();
        }

        private static bool IsControllerInternal(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
        {
            if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                return actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(InternalApiAttribute), true).Any();
            return false;
        }
    }
}
