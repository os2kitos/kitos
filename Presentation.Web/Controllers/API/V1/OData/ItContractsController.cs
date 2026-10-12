using System;
using System.Linq;
using System.Net;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Core.DomainServices.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Core.DomainServices.Extensions;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItContractsController : BaseEntityController<ItContract>
    {
        public ItContractsController(IGenericRepository<ItContract> repository)
            : base(repository)
        {
        }

        /// <summary>
        /// Hvis den autentificerede bruger er Global Admin, returneres alle kontrakter.
        /// Ellers returneres de kontrakter som brugeren har rettigheder til at se.
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        [Route("odata/ItContracts")]
        [RequireTopOnOdataThroughKitosToken]
        public override IActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter alle organisationens IT Kontrakter
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 3)]
        [Route("odata/Organizations({key})/ItContracts")]
        [RequireTopOnOdataThroughKitosToken]
        public IActionResult GetItContracts(int key)
        {
            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(key);
            if (organizationDataReadAccessLevel != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable().ByOrganizationId(key);

            return Ok(result.ToList());
        }

        [NonAction]
        public override IActionResult Post(int organizationId, ItContract entity) => throw new NotSupportedException();
    }
}




