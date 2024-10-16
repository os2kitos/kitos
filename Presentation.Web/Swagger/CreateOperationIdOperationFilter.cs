using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class CreateOperationIdOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var isCollectionResult = false;
            if (operation.responses.TryGetValue("200", out var okResponse))
            {
                isCollectionResult = okResponse.schema?.type?.Equals("array", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            var responseTypeNamePart = isCollectionResult ? "Many" : "Single";

            var opsId =
                $"{apiDescription.HttpMethod.Method.ToLowerInvariant()}_{responseTypeNamePart}_{apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName}_{apiDescription.ActionDescriptor.ActionName}";

            if (apiDescription.ActionDescriptor is ReflectedHttpActionDescriptor actionDescriptor)
            {
                var methodInfo = actionDescriptor.MethodInfo;
                var publicMethodsWithSameName = methodInfo
                    .DeclaringType
                    .GetMethods()
                    //Only match public methods
                    .Where(x => x.IsPublic)
                    //Don't care about omitted methods
                    .Where(x => x.GetCustomAttributes(false).Contains(typeof(NonActionAttribute)) == false)
                    //Match similar names regardless of casing
                    .Where(x => x.Name.Equals(methodInfo.Name, StringComparison.OrdinalIgnoreCase))
                    //Order by declaring order
                    .OrderBy(m => m.Name)
                    .ThenBy(m => string.Join(",", m.GetParameters().Select(p => p.ParameterType.FullName)))
                    .ToList();

                var indexOfCurrentAction = publicMethodsWithSameName.IndexOf(methodInfo);
                if (publicMethodsWithSameName.Count > 1 && indexOfCurrentAction != 0)
                {
                    opsId += "_V" + indexOfCurrentAction;
                }
            }

            operation.operationId = opsId;
        }
    }
}