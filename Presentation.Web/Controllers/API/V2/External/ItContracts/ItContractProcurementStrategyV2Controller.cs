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
    [RoutePrefix("api/v2/it-contract-procurement-strategy-types")]
    public class ItContractProcurementStrategyV2Controller : BaseRegularOptionTypeV2Controller<ItContract, ProcurementStrategyType>
    {
        public ItContractProcurementStrategyV2Controller(IOptionsApplicationService<ItContract, ProcurementStrategyType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract procurement strategy type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the procurement strategy types availability</param>
        /// <returns>A list of available It-Contract procurement strategy types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-Contract procurement strategy type
        /// </summary>
        /// <param name="procurementStrategyTypeUuid">procurement strategy type identifier</param>
        /// <param name="organizationUuid">organization context for the procurement strategy type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the procurement strategy type is available in the organization</returns>
        [HttpGet]
        [Route("{procurementStrategyTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid procurementStrategyTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(procurementStrategyTypeUuid, organizationUuid);
        }
    }
}