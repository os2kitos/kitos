using Infrastructure.Services.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Presentation.Web.Swagger
{
    /// <summary>
    /// Adds the Bearer security requirement to every operation so that Swagger UI
    /// automatically includes the Authorization header when a token has been entered.
    /// </summary>
    public class AddBearerSecurityRequirementOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference(AuthenticationSchemes.Bearer, context.Document), new List<string>() }
            });
        }
    }
}
