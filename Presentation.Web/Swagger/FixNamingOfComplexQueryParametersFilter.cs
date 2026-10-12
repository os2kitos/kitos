using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Presentation.Web.Swagger
{
    /// <summary>
    /// Fix the addition of complex query param name in controllers. (fixed in swagger 3.0)
    /// https://github.com/domaindrivendev/Swashbuckle.WebApi/issues/1038
    /// </summary>
    public class FixNamingOfComplexQueryParametersFilter : IOperationFilter
    {
        private const string Separator = ".";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) return;
            if (!MatchGET(context)) return;

            foreach (var parameter in operation.Parameters)
            {
                if (parameter is OpenApiParameter mutableParam && mutableParam.Name?.Contains(Separator) == true)
                {
                    mutableParam.Name = mutableParam.Name.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries).Last();
                }
            }
        }

        private static bool MatchGET(OperationFilterContext context) =>
            string.Equals(context.ApiDescription.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase);
    }
}
