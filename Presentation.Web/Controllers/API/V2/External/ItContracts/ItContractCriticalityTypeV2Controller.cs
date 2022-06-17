using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [RoutePrefix("api/v2/it-contract-criticality-types")]
    public class ItContractCriticalityTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, CriticalityType>
    {
        public ItContractCriticalityTypeV2Controller(IOptionsApplicationService<ItContract, CriticalityType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract criticality type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the criticality types availability</param>
        /// <returns>A list of available It-Contract criticality types</returns>
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
        /// Returns requested It-Contract criticality type
        /// </summary>
        /// <param name="criticalityTypeUuid">criticality type identifier</param>
        /// <param name="organizationUuid">organization context for the criticality type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the criticality type is available in the organization</returns>
        [HttpGet]
        [Route("{purchaseTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid criticalityTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(criticalityTypeUuid, organizationUuid);
        }
    }
}