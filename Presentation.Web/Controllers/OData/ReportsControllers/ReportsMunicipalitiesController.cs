using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ReportsMunicipalitiesController : BaseOdataAuthorizationController<Organization>
    {
        public ReportsMunicipalitiesController(IGenericRepository<Organization> repository)
            : base(repository){
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("ReportsMunicipalities")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IOrderedEnumerable<Organization>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult Get()
        {
            if (AuthorizationContext.GetCrossOrganizationReadAccess() != CrossOrganizationDataReadAccessLevel.All)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            var result = Repository.Get().Where(m => m.TypeId == 1);
            return Ok(result.OrderBy(r => r.Name));
        }
    }
}
