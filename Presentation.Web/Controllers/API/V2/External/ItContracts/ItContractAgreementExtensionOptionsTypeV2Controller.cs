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
    [RoutePrefix("api/v2/it-contract-agreement-extension-option-types")]
    public class ItContractAgreementExtensionOptionTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, OptionExtendType>
    {
        public ItContractAgreementExtensionOptionTypeV2Controller(IOptionsApplicationService<ItContract, OptionExtendType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract agreement extension option type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the agreement extension option types availability</param>
        /// <returns>A list of available It-Contract agreement extension option types</returns>
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
        /// Returns requested It-Contract agreement extension option type
        /// </summary>
        /// <param name="agreementExtensionOptionTypeUuid">agreement extension option type identifier</param>
        /// <param name="organizationUuid">organization context for the agreement extension option type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the agreement extension option type is available in the organization</returns>
        [HttpGet]
        [Route("{agreementExtensionOptionTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid agreementExtensionOptionTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(agreementExtensionOptionTypeUuid, organizationUuid);
        }
    }
}