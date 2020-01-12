using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class OrganizationUnitsController : BaseEntityController<OrganizationUnit>
    {
        public OrganizationUnitsController(IGenericRepository<OrganizationUnit> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        [ODataRoute("OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits()
        {
            return base.Get();
        }

        //GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits(int orgKey)
        {
            if (GetOrganizationReadAccessLevel(orgKey) < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == orgKey);
            return Ok(result);
        }
    }
}
