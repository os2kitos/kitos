using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.OData.ReportsControllers
{
    [InternalApi]
    public class ReportsItSystemRolesController : BaseOdataAuthorizationController<ItSystemRole>
    {
        public ReportsItSystemRolesController(IGenericRepository<ItSystemRole> repository)
            : base(repository) { }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsItSystemRoles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IOrderedEnumerable<ItSystemRole>>))]
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
