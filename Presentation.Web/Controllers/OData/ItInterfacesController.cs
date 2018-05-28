using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfacesController : BaseEntityController<ItInterface>
    {
        private readonly IAuthenticationService _authService;

        public ItInterfacesController(IGenericRepository<ItInterface> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _authService = authService;
        }

        [EnableQuery]
        [ODataRoute("ItInterfaces")]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        // GET /Organizations(1)/ItInterfaces
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItInterfaces")]
        public IHttpActionResult GetItInterfaces(int key)
        {
            //var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            //if (!_authService.HasReadAccessOutsideContext(UserId))
            //{
            //    if (loggedIntoOrgId != key)
            //        return StatusCode(HttpStatusCode.Forbidden);

            //    var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            //    return Ok(result);
            //}
            //else
            //{
            //    var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
            //    return Ok(result);
            //}
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
            return Ok(result);
        }

        // GET /Organizations(1)/ItInterfaces(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItInterfaces({interfaceKey})")]
        public IHttpActionResult GetItInterfaces(int orgKey, int interfaceKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.OrganizationId == orgKey && m.Id == interfaceKey);
            if (entity == null)
                return NotFound();

            if (_authService.HasReadAccess(UserId, entity))
                return Ok(entity);

            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
