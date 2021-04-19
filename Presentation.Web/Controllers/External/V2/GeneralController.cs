using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Controllers.API;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class GeneralController : BaseApiController
    {
        [HttpGet]
        [Route("business-types")]
        public HttpResponseMessage GetBusinessTypes()
        {
            return Ok();
        }
    }
}