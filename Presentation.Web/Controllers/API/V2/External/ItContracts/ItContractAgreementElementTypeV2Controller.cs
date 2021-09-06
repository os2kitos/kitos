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
    [RoutePrefix("api/v2/it-contract-agreement-element-types")]
    public class ItContractAgreementElementTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, AgreementElementType>
    {
        public ItContractAgreementElementTypeV2Controller(IOptionsApplicationService<ItContract, AgreementElementType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract agreement element type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the agreement element types availability</param>
        /// <returns>A list of available It-Contract agreement element types</returns>
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
        /// Returns requested It-Contract agreement element type
        /// </summary>
        /// <param name="agreementElementTypeUuid">agreement element type identifier</param>
        /// <param name="organizationUuid">organization context for the agreement element type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the agreement element type is available in the organization</returns>
        [HttpGet]
        [Route("{agreementElementTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid agreementElementTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(agreementElementTypeUuid, organizationUuid);
        }
    }
}