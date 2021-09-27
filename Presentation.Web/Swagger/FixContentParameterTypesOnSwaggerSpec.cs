using System.Web.Http.Description;
using Presentation.Web.Helpers;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class FixContentParameterTypesOnSwaggerSpec : IOperationFilter
    {
        private const string json = "application/json";
        private const string jsonMergePatch = "application/merge-patch+json"; //https://datatracker.ietf.org/doc/html/rfc7396

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.consumes.Remove(jsonMergePatch);
            operation.produces.Remove(jsonMergePatch);
            if (apiDescription.RelativePath.IsExternalApiPath())
            {
                operation.produces.Clear();
                operation.consumes.Clear();
                switch (apiDescription.HttpMethod.Method.ToUpperInvariant())
                {
                    case "POST":
                    case "PUT":
                        operation.consumes.Add(json);
                        operation.produces.Add(json);
                        break;
                    case "PATCH":
                        operation.consumes.Add(jsonMergePatch);
                        operation.consumes.Add(json);
                        operation.produces.Add(json);
                        break;
                    case "GET":
                        operation.produces.Add(json);
                        break;
                    case "DELETE": //Intended fallthrough
                    default:
                        break;
                }


            }
        }
    }
}