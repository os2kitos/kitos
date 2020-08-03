using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItSystemsController : BaseEntityController<ItSystem>
    {
        public ItSystemsController(IGenericRepository<ItSystem> repository)
            : base(repository)
        {
        }

        /// <summary>
        /// Henter alle organisationens IT-Systemer samt offentlige IT-systemer fra andre organisationer.
        /// Resultatet filtreres i hht. brugerens læserettigheder i den opgældende organisation, samt på tværs af organisationer.
        /// </summary>
        /// <param name="orgKey"></param>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystem>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            var readAccessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (readAccessLevel == OrganizationDataReadAccessLevel.None)
            {
                return Forbidden();
            }

            var result = Repository
                    .AsQueryable()
                    .ByOrganizationDataAndPublicDataFromOtherOrganizations(orgKey, readAccessLevel, GetCrossOrganizationReadAccessLevel());

            return Ok(result);
        }

        [ODataRoute("ItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystem>>))]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        public override IHttpActionResult Delete(int key)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }

        public override IHttpActionResult Post(ItSystem entity)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }
    }
}
