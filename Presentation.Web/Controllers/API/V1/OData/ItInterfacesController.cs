using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.ApplicationServices.Extensions;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
    public class ItInterfacesController : BaseEntityController<ItInterface>
    {
        public ItInterfacesController(IGenericRepository<ItInterface> repository)
            : base(repository)
        {
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("ItInterfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<ItInterface>))]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter alle snitflader i organisationen samt offentlige snitflader i andre organisationer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/ItInterfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItInterface>>))]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItInterfaces(int key)
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

            return Ok(result);
        }

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Post(int organizationId, ItInterface entity) => throw new NotSupportedException();
    }
}
