using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [RoutePrefix("api/v2/it-contract-handover-trial-types")]
    public class ItContractHandoverTrialTypeV2Controller : BaseRegularOptionTypeV2Controller<HandoverTrial, HandoverTrialType>
    {
        public ItContractHandoverTrialTypeV2Controller(IOptionsApplicationService<HandoverTrial, HandoverTrialType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract handover trial type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the handover trial types availability</param>
        /// <returns>A list of available It-Contract handover trial types</returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-Contract handover trial type
        /// </summary>
        /// <param name="handoverTrialTypeUuid">handover trial type identifier</param>
        /// <param name="organizationUuid">organization context for the handover trial type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the handover trial type is available in the organization</returns>
        [HttpGet]
        [Route("{handoverTrialTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid handoverTrialTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(handoverTrialTypeUuid, organizationUuid);
        }
    }
}