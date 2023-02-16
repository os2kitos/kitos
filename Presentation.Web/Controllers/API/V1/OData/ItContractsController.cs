using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using Core.DomainServices.Authorization;
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
        [ODataRoute("ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter alle organisationens IT Kontrakter
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItContracts(int key)
        {
            var organizationDataReadAccessLevel = GetOrganizationReadAccessLevel(key);
            if (organizationDataReadAccessLevel != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable().ByOrganizationId(key);

            return Ok(result);
        }

        [NonAction]
        public override IHttpActionResult Post(int organizationId, ItContract entity) => throw new NotSupportedException();
    }
}
