using System;
using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Net;
using System.Security;
using System.Threading;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using System.Linq;
using Presentation.Web.Controllers.OData.ReportsControllers;
using Core.DomainModel.ItSystem;
using System.Collections.Generic;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.OData
{
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
