using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    [RoutePrefix("api/v2/it-system-usage-relation-frequency-types")]
    public class ItSystemUsageRelationFrequencyTypeV2Controller : BaseRegularOptionTypeV2Controller<SystemRelation, RelationFrequencyType>
    {
        public ItSystemUsageRelationFrequencyTypeV2Controller(IOptionsApplicationService<SystemRelation, RelationFrequencyType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns IT-System usage relation frequency option types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the relation frequency type availability</param>
        /// <returns>A list of available IT-System usage relation frequency option types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested IT-System usage relation frequency option type
        /// </summary>
        /// <param name="relationFrequencyTypeUuid">relation frequency type identifier</param>
        /// <param name="organizationUuid">organization context for the relation frequency type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the relation frequency type is available in the organization</returns>
        [HttpGet]
        [Route("{relationFrequencyTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid relationFrequencyTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(relationFrequencyTypeUuid, organizationUuid);
        }
    }
}