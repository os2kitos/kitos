using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class ItSystemUsageMigration : BaseApiController
    {

        public HttpResponseMessage GetUnusedItSystemsBySearchAndOrganization(int organizationId, string q, int limit)
        {
            var newDTO = new List<ItSystemDTO>();
            return CreateResponse(HttpStatusCode.OK, newDTO);
        }

        public HttpResponseMessage GetMigrationConflicts(int fromSystemId, int toSystemId)
        {
            var newDTO = new ItSystemUsageMigrationDTO();
            return CreateResponse(HttpStatusCode.OK, newDTO);
        }

        public HttpResponseMessage ExecuteMigration(ItSystemUsageMigrationDTO toExecute)
        {
            return CreateResponse(HttpStatusCode.OK);
        }

    }
}