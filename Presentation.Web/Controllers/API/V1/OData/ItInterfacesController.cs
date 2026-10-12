using System;
using System.Linq;
using System.Net;
using Core.ApplicationServices.Extensions;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItInterfacesController : BaseEntityController<ItInterface>
    {
        public ItInterfacesController(IGenericRepository<ItInterface> repository)
            : base(repository)
        {
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [Route("odata/ItInterfaces")]
        [RequireTopOnOdataThroughKitosToken]
        public override IActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter alle snitflader i organisationen samt offentlige snitflader i andre organisationer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 3)]
        [Route("odata/Organizations({key})/ItInterfaces")]
        [RequireTopOnOdataThroughKitosToken]
        public IActionResult GetItInterfaces(int key)
        {
            var result = Repository
                .AsQueryable()
                .ByOrganizationDataQueryParameters(
                    new OrganizationDataQueryParameters(
                        key,
                        OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations,
                        AuthorizationContext.GetDataAccessLevel(key)
                    )
                );

            return Ok(result.ToList());
        }

        [NonAction]
        public override IActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Post(int organizationId, ItInterface entity) => throw new NotSupportedException();
    }
}




