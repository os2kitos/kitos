using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class CreateOperationIdOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.operationId =
                apiDescription.HttpMethod.Method + "_" +
                apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName + "_" +
                apiDescription.ActionDescriptor.ActionName + "_" +
                DescribeParameters(apiDescription.ActionDescriptor.ActionBinding.ParameterBindings);
        }

        private static string DescribeParameters(IEnumerable<HttpParameterBinding> actionBindingParameterBindings)
        {
            var parameterDescriptions = actionBindingParameterBindings
                .Select(DescribeParameter)
                .ToList();

            return parameterDescriptions.Any() ? $"_{string.Join("_", parameterDescriptions)}" : string.Empty;
        }

        private static string DescribeParameter(HttpParameterBinding actionBindingParameterBinding)
        {
            return $"{actionBindingParameterBinding.Descriptor.ParameterType.Name}-{actionBindingParameterBinding.Descriptor.ParameterName}";
        }
    }
}