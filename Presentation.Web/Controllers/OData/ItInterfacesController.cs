using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<ItInterface>))]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter alle snitflader i organisationen samt offentlige snitflader i andre organisationer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItInterfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItInterface>>))]
        public IHttpActionResult GetItInterfaces(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
            return Ok(result);
        }
    }
}
