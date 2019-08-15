using Core.ApplicationServices;
using Core.DomainServices;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Linq;
using Presentation.Web.Controllers.OData.ReportsControllers;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ReportsItSystemRolesController : BaseOdataAuthorizationController<ItSystemRole>
    {
        private readonly IAuthenticationService _authService;
        public ReportsItSystemRolesController(IGenericRepository<ItSystemRole> repository, IAuthenticationService authService)
            : base(repository){
            _authService = authService;
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsItSystemRoles")]
        public IHttpActionResult Get()
        {
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            var result = Repository.Get().OrderBy(i => i.Name);
            return Ok(result);
        }
    }
}
