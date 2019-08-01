using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class KITOSApiExplorer : ApiExplorer
    {
        public KITOSApiExplorer(HttpConfiguration configuration) : base(configuration)
        {

        }

        public override bool ShouldExploreController(string controllerVariableValue,
            HttpControllerDescriptor controllerDescriptor, IHttpRoute route)
        {
            if (controllerDescriptor.GetCustomAttributes<InternalApiAttribute>().Any())
            {
                return false;
            }
            else if (controllerDescriptor.GetCustomAttributes<PublicApiAttribute>().Any())
            {
                return true;
            }
            else
            {
                return base.ShouldExploreController(controllerVariableValue, controllerDescriptor, route);
            }
        }

    }
}