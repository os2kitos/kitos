using System.Diagnostics;
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

        public override bool ShouldExploreAction(string actionVariableValue, HttpActionDescriptor actionDescriptor, IHttpRoute route)
        {
            if (actionDescriptor.GetCustomAttributes<InternalApiAttribute>().Any())
            {
                return false;
            }
            else if (actionDescriptor.GetCustomAttributes<PublicApiAttribute>().Any())
            {
                return true;
            }
            else
            {
                Debug.WriteLine("Api action er ikke markeret som tilgængelig eller låst");
                return false;
                //throw new ApiNotMarkedException("Api action er ikke markeret som tilgængelig eller låst");
            }
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
                Debug.WriteLine("Api controller er ikke markeret som tilgængelig eller låst");
                return base.ShouldExploreController(controllerVariableValue, controllerDescriptor, route);
                //throw new ApiNotMarkedException("Api controller er ikke markeret som tilgængelig eller låst");
            }
        }

    }
}