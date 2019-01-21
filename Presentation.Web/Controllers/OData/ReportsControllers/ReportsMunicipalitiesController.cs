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

namespace Presentation.Web.Controllers.OData
{
    public class ReportsMunicipalitiesController : BaseOdataAuthorizationController<Organization>
    {
        private readonly IAuthenticationService _authService;
        public ReportsMunicipalitiesController(IGenericRepository<Organization> repository, IAuthenticationService authService)
            : base(repository){
            _authService = authService;
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsMunicipalities")]
        public IHttpActionResult Get()
        {
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            var result = Repository.Get().Where(m => m.TypeId == 1);
            return Ok(result.OrderBy(r => r.Name));
        }
    }
}
