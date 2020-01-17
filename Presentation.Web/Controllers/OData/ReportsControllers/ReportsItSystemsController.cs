using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [InternalApi]
    public class ReportsItSystemsController : BaseOdataAuthorizationController<ItSystem>
    {
        public ReportsItSystemsController(IGenericRepository<ItSystem> repository)
            : base(repository){
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IOrderedEnumerable<ItSystem>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult Get()
        {
            if (AuthorizationContext.GetCrossOrganizationReadAccess() != CrossOrganizationDataReadAccessLevel.All)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            var result = Repository.Get().OrderBy(i => i.Name);
            return Ok(result);
        }
    }
}
