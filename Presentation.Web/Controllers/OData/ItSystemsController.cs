using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Ninject.Infrastructure.Language;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItSystemsController : BaseEntityController<ItSystem>
    {
        public ItSystemsController(IGenericRepository<ItSystem> repository, IAuthenticationService authService, IAuthorizationContext authorizationContext)
            : base(repository, authService, authorizationContext)
        {
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems")]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            if (!AllowOrganizationAccess(orgKey))
            { 
                return Forbidden();
            }

            var result = Repository.AsQueryable(readOnly:true).Where(m => m.OrganizationId == orgKey);

            var systemsWithAllowedReadAccess  = result.ToEnumerable().Where(AllowReadAccess);

            return Ok(systemsWithAllowedReadAccess);
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems({sysKey})")]
        public IHttpActionResult GetItSystems(int orgKey, int sysKey)
        {
            var system = Repository.GetByKey(sysKey);
            if (!AllowReadAccess(system))
            {
                return Forbidden();
            }
            if (system == null)
            {
                return NotFound();
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
