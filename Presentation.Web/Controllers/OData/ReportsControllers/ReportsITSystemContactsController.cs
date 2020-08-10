using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [InternalApi]
    public class ReportsITSystemContactsController : BaseOdataAuthorizationController<ItSystemRight>
    {
        public ReportsITSystemContactsController(IGenericRepository<ItSystemRight> repository)
            : base(repository)
        {
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsITSystemContacts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ReportItSystemRightOutputDTO>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult Get()
        {
            if (AuthorizationContext.GetCrossOrganizationReadAccess() != CrossOrganizationDataReadAccessLevel.All)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            var result = Repository.Get();
            try
            {
                var dtos = Mapper.Map<IEnumerable<ItSystemRight>, IEnumerable<ReportItSystemRightOutputDTO>>(result);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}
