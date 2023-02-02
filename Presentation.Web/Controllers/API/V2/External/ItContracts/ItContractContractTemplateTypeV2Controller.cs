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

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [RoutePrefix("api/v2/it-contract-contract-template-types")]
    public class ItContractContractTemplateTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, ItContractTemplateType>
    {
        public ItContractContractTemplateTypeV2Controller(IOptionsApplicationService<ItContract, ItContractTemplateType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract contract template type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the contract template types availability</param>
        /// <returns>A list of available It-Contract contract template types</returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-Contract contract template type
        /// </summary>
        /// <param name="contractTemplateTypeUuid">contract template type identifier</param>
        /// <param name="organizationUuid">organization context for the contract template type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the contract template type is available in the organization</returns>
        [HttpGet]
        [Route("{contractTemplateTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid contractTemplateTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(contractTemplateTypeUuid, organizationUuid);
        }
    }
}