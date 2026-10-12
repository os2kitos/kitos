using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class CreateOperationIdOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isCollectionResult = false;
            var responses = operation.Responses ?? [];
            if (responses.TryGetValue("200", out var okResponse))
            {
                foreach (var mediaType in okResponse.Content?.Values ?? [])
                {
                    if (mediaType.Schema?.Type == JsonSchemaType.Array)
                    {
                        isCollectionResult = true;
                        break;
                    }
                }
            }

            var responseTypeNamePart = isCollectionResult ? "Many" : "Single";
            var apiDescription = context.ApiDescription;
            var httpMethod = apiDescription.HttpMethod ?? "GET";

            if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerDescriptor)
            {
                operation.OperationId = $"{httpMethod.ToLowerInvariant()}_{responseTypeNamePart}__";
                return;
            }

            var controllerName = controllerDescriptor.ControllerName;
            var actionName = controllerDescriptor.ActionName;
            var opsId = $"{httpMethod.ToLowerInvariant()}_{responseTypeNamePart}_{controllerName}_{actionName}";

            var methodInfo = controllerDescriptor.MethodInfo;
            var publicMethodsWithSameName = methodInfo
                .DeclaringType?
                .GetMethods()
                .Where(x => x.IsPublic)
                .Where(x => x.Name.Equals(methodInfo.Name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Name)
                .ThenBy(m => string.Join(",", m.GetParameters().Select(p => p.ParameterType.FullName)))
                .ToList();

            if (publicMethodsWithSameName != null)
            {
                var indexOfCurrentAction = publicMethodsWithSameName.IndexOf(methodInfo);
                if (publicMethodsWithSameName.Count > 1 && indexOfCurrentAction != 0)
                {
                    opsId += "_V" + indexOfCurrentAction;
                }
            }

            var relativePath = apiDescription.RelativePath ?? string.Empty;
            if (relativePath.Contains("/$count", StringComparison.OrdinalIgnoreCase))
            {
                opsId += "_count";
            }

            operation.OperationId = opsId;
        }
    }
}
