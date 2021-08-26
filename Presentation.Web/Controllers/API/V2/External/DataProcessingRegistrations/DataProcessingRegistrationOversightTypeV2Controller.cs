using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations
{
    [RoutePrefix("api/v2/data-processing-registrations-oversight-types")]
    public class DataProcessingRegistrationOversightTypeV2Controller : BaseRegularOptionTypeV2Controller<DataProcessingRegistration, DataProcessingOversightOption>
    {
        public DataProcessingRegistrationOversightTypeV2Controller(IOptionsApplicationService<DataProcessingRegistration, DataProcessingOversightOption> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns Data Processing Registration oversight options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the oversight availability</param>
        /// <returns>A list of available Data Processing Registration oversight</returns>
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
        /// Returns requested Data Processing Registration oversight
        /// </summary>
        /// <param name="oversightUuid">oversight identifier</param>
        /// <param name="organizationUuid">organization context for the oversight availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the oversight is available in the organization</returns>
        [HttpGet]
        [Route("{oversightUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid oversightUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(oversightUuid, organizationUuid);
        }
    }
}