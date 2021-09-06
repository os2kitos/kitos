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
    [RoutePrefix("api/v2/it-contract-notice-period-month-types")]
    public class ItContractNoticePeriodMonthTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, TerminationDeadlineType>
    {
        public ItContractNoticePeriodMonthTypeV2Controller(IOptionsApplicationService<ItContract, TerminationDeadlineType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract notice period month type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the notice period month types availability</param>
        /// <returns>A list of available It-Contract notice period month types</returns>
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
        /// Returns requested It-Contract notice period month type
        /// </summary>
        /// <param name="noticePeriodMonthTypeUuid">notice period month type identifier</param>
        /// <param name="organizationUuid">organization context for the notice period month type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the notice period month type is available in the organization</returns>
        [HttpGet]
        [Route("{noticePeriodMonthTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid noticePeriodMonthTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(noticePeriodMonthTypeUuid, organizationUuid);
        }
    }
}