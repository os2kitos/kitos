using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Ninject.Infrastructure.Language;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Context;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

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
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(ODataResponse<IEnumerable<ItSystem>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            if (!AllowOrganizationAccess(orgKey))
            { 
                return Forbidden();
            }

            var result = Repository.AsQueryable(readOnly:true).Where(m => m.OrganizationId == orgKey);

            var systemsWithAllowedReadAccess  = result.ToEnumerable().Where(AllowRead);

            return Ok(systemsWithAllowedReadAccess);
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems({sysKey})")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<ItSystem>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystems(int orgKey, int sysKey)
        {
            var system = Repository.GetByKey(sysKey);
            if (!AllowRead(system))
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystem>>))]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }
    }
}
