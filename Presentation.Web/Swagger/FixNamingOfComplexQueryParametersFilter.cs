using System;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    /// <summary>
    /// Fix the addition of complex query param name in controllers. (fixed in swagger 3.0)
    /// https://github.com/domaindrivendev/Swashbuckle.WebApi/issues/1038
    /// </summary>
    public class FixNamingOfComplexQueryParametersFilter : IOperationFilter
    {
        private const string Separator = ".";

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters != null && MatchGET(apiDescription))
            {
                foreach (var parameter in operation.parameters.Where(IsNestedInComplexType).ToList())
                {
                    parameter.name = GetParameterNameWithoutPrefix(parameter);
                }
            }
        }

        private static string GetParameterNameWithoutPrefix(Parameter parameter)
        {
            return parameter.name.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        private static bool IsNestedInComplexType(Parameter x)
        {
            return x.name.Contains(Separator);
        }

        private static bool MatchGET(ApiDescription apiDescription)
        {
            return apiDescription.HttpMethod.Method.Equals("get", StringComparison.OrdinalIgnoreCase);
        }
    }
}