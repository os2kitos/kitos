using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.External.V2;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    [RoutePrefix("api/v2/it-system-usage-registered-data-category-types")]
    public class ItSystemUsageRegisteredDataCategoryTypeV2Controller : BaseOptionTypeV2Controller<ItSystemUsage, RegisterType>
    {
        public ItSystemUsageRegisteredDataCategoryTypeV2Controller(IOptionsApplicationService<ItSystemUsage, RegisterType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage registered data category types 
        /// </summary>
        /// <param name="organizationUuid">organization context for the type availability</param>
        /// <returns>A list of available It-System Usage register types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get(Guid organizationUuid, [FromUri] StandardPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System Usage registered data category type
        /// </summary>
        /// <param name="registerTypeUuid">register type identifier</param>
        /// <param name="organizationUuid">organization context for the register type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the register type is available in the organization</returns>
        [HttpGet]
        [Route("{registerTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AvailableNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get(Guid registerTypeUuid, Guid organizationUuid)
        {
            return GetSingle(registerTypeUuid, organizationUuid);
        }
    }
}