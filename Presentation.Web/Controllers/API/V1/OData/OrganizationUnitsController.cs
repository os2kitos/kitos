using System;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
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

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey);

            return Ok(result);
        }

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Post(int organizationId, OrganizationUnit entity) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<OrganizationUnit> delta) => throw new NotSupportedException();
    }
}
