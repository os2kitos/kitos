using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemUsageMigrationController : ApiController
    {
        
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetUnusedItSystemsBySearchAndOrganization(int organizationId, string q, int limit)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetMigrationConflicts(int fromSystemId, int toSystemId)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage ExecuteMigration(ItSystemUsageMigrationDTO toExecute)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}