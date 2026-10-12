using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository)
            : base(repository)
        {
        }

        /// <summary>
        /// Henter alle organisationens IT-Systemanvendelser.
        /// </summary>
        /// <param name="orgKey"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [Route("odata/Organizations({orgKey})/ItSystemUsages")]
        [RequireTopOnOdataThroughKitosToken]
        public IActionResult GetItSystems(int orgKey)
        {
            //Usages are local so full access is required
            var accessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (accessLevel < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey);

            return Ok(result.ToList());
        }
    }
}




