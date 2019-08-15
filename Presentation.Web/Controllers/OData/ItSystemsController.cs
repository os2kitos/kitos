using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Access;

namespace Presentation.Web.Controllers.OData
{
    public partial class ItSystemsController : BaseEntityController<ItSystem>
    {
        private readonly IOrganizationContextFactory _contextFactory;

        public ItSystemsController(IGenericRepository<ItSystem> repository, IAuthenticationService authService, IOrganizationContextFactory contextFactory)
            : base(repository, authService)
        {
            _contextFactory = contextFactory;
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems")]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            var organizationContext = _contextFactory.CreateOrganizationContext(orgKey);
            if (!organizationContext.AllowReads(UserId))
            { 
                return Forbidden();
            }

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == orgKey || m.AccessModifier == AccessModifier.Public);
            
            return Ok(result);
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems({sysKey})")]
        public IHttpActionResult GetItSystems(int orgKey, int sysKey)
        {
            var system = Repository.AsQueryable().SingleOrDefault(m => m.Id == sysKey);
            if (system == null)
            {
                return NotFound();
            }

            var organizationContext = _contextFactory.CreateOrganizationContext(orgKey);
            if (!organizationContext.AllowReads(UserId, system))
            {
                return Forbidden();
            }

            return Ok(system);
        }

        [ODataRoute("ItSystems")]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }
    }
}
