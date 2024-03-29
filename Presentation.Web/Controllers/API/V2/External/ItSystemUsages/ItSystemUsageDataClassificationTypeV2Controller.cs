﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [RoutePrefix("api/v2/it-system-usage-data-classification-types")]
    public class ItSystemUsageDataClassificationTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, ItSystemCategories>
    {
        public ItSystemUsageDataClassificationTypeV2Controller(IOptionsApplicationService<ItSystemUsage, ItSystemCategories> optionService) 
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns IT-System usage data classification option types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the data classification type availability</param>
        /// <returns>A list of available IT-System usage data classification option types</returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested IT-System usage data classification option type
        /// </summary>
        /// <param name="dataClassificationTypeUuid">data classification type identifier</param>
        /// <param name="organizationUuid">organization context for the data classification type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the data classification type is available in the organization</returns>
        [HttpGet]
        [Route("{dataClassificationTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid dataClassificationTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(dataClassificationTypeUuid, organizationUuid);
        }
    }
}