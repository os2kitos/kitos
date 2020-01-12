using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
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
        public ItSystemsController(IGenericRepository<ItSystem> repository, IAuthenticationService authService, IAuthorizationContext authorizationContext)
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
        public override IHttpActionResult Get()
        {
            return base.Get();
        }
    }
}
