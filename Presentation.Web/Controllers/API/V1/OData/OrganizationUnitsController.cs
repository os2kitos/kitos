using System.Linq;
using System;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
        [Route("odata/OrganizationUnits")]
        public IActionResult GetOrganizationUnits()
        {
            return base.Get();
        }

        //GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [Route("odata/Organizations({orgKey})/OrganizationUnits")]
        public IActionResult GetOrganizationUnits(int orgKey)
        {
            if (GetOrganizationReadAccessLevel(orgKey) < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey);

            return Ok(result.ToList());
        }

        [NonAction]
        public override IActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Post(int organizationId, OrganizationUnit entity) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Patch(int key, Delta<OrganizationUnit> delta) => throw new NotSupportedException();
    }
}




