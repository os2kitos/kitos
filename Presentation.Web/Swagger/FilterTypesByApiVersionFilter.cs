using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace Presentation.Web.Swagger
{
    public class FilterTypesByApiVersionFilter : IOperationFilter
    {
        private readonly Func<SwaggerDocument, int> _getApiVersion;
        private readonly Func<string, int> _getPathApiVersion;

        public FilterTypesByApiVersionFilter(Func<SwaggerDocument, int> getApiVersion, Func<string, int> getPathApiVersion)
        {
            _getApiVersion = getApiVersion;
            _getPathApiVersion = getPathApiVersion;
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var test = apiDescription.Route;
            
            /* foreach (var path in swaggerDoc.paths.ToList())
             {
                 if (docVersion != _getPathApiVersion(path.Key))
                 {
                     swaggerDoc.paths.Remove(path);
                 }
             }*/
        }

        /*private readonly Func<string, int> _getPathApiVersion;

        public FilterTypesByApiVersionFilter(Func<string, int> getPathApiVersion)
        {
            _getPathApiVersion = getPathApiVersion;
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                return;
            }
            apiDescription.
            foreach (var parameter in operation.parameters.OfType<OpenApiParameter>())
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                var routeInfo = description.ParameterDescriptor.;

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (routeInfo == null)
                {
                    continue;
                }

                if (parameter.Default == null)
                {
                    parameter.Default = routeInfo.DefaultValue;
                }

                parameter.Required |= !routeInfo.IsOptional;
            }
        }*/
    }
}