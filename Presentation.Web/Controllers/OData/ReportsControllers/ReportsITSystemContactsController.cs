using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [InternalApi]
    [DeprecatedApi]
    public class ReportsITSystemContactsController : BaseOdataAuthorizationController<ItSystemRight>
    {
        private readonly IAuthenticationService _authService;
        public ReportsITSystemContactsController(IGenericRepository<ItSystemRight> repository, IAuthenticationService authService)
            : base(repository){
            _authService = authService;
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsITSystemContacts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ReportItSystemRightOutputDTO>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult Get()
        {
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            var result = Repository.Get();
            try {
            var dtos = AutoMapper.Mapper.Map<IEnumerable<ItSystemRight>, IEnumerable<ReportItSystemRightOutputDTO>>(result);
                return Ok(dtos);
            }
            catch(Exception e)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}
