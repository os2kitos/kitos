using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.ItSystemUsageMigration;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemUsageMigrationController : BaseApiController
    {
        private IItSystemUsageMigrationService _itSystemUsageMigrationService;

        public ItSystemUsageMigrationController(IItSystemUsageMigrationService itSystemUsageMigrationService, IAuthorizationContext authContext) : base(authContext)
        {
            _itSystemUsageMigrationService = itSystemUsageMigrationService;
        }

        [HttpGet]
        //[Route("api/ItSystemUsageMigration/")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetMigrationConflicts(int fromSystemId, int toSystemId)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        //[Route("api/ItSystemUsageMigration/")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage ExecuteMigration(ItSystemUsageMigrationDTO toExecute)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        //[Route("api/ItSystemUsageMigration/UnusedItSystems/")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(IEnumerable<ItSystem>))]
        public HttpResponseMessage GetUnusedItSystemsBySearchAndOrganization(int organizationId, string q, int limit)
        {
            try
            {
                var result = _itSystemUsageMigrationService.GetUnusedItSystemsByOrganization(organizationId, q, limit);
                return CreateResponse(result.StatusCode, MapList<ItSystem, ItSystemDTO>(result.ReturnValue));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private IEnumerable<TOut> MapList<TIn, TOut>(IEnumerable<TIn> input)
        {
            return Map<IEnumerable<TIn>, IEnumerable<TOut>>(input);
        }
    }
}