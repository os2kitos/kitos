using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation;
using Presentation.Web;
using Presentation.Web.Controllers;
using Presentation.Web.Controllers.External;
using Presentation.Web.Controllers.External.V2;
using Presentation.Web.Controllers.External.V2.ItSystemUsages;
using Presentation.Web.Controllers.External.V2.ItSystemUsages.SystemRelations;
using Presentation.Web.Models.External.V2;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages.SystemRelations
{
    [RoutePrefix("api/v2/it-system-sensitive-personal-data-types")]
    public class ItSystemSensitivePersonalDataTypeV2Controller : BaseOptionTypeV2Controller<ItSystem, SensitivePersonalDataType>
    {
        public ItSystemSensitivePersonalDataTypeV2Controller(IOptionsApplicationService<ItSystem, SensitivePersonalDataType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System sensitive personal data types 
        /// </summary>
        /// <param name="organizationUuid">organization context for the sensitive personal data types availability</param>
        /// <returns>A list of available It-System sensitive personal data types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemSensitivePersonalDataTypes(Guid organizationUuid, [FromUri] StandardPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System sensitive personal data type
        /// </summary>
        /// <param name="sensitivePersonalDataTypeUuid">sensitive personal data type identifier</param>
        /// <param name="organizationUuid">organization context for the sensitive personal data type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the sensitive personal data type is available in the organization</returns>
        [HttpGet]
        [Route("{sensitivePersonalDataTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AvailableNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemSensitivePersonalDataType(Guid sensitivePersonalDataTypeUuid, Guid organizationUuid)
        {
            return GetSingle(sensitivePersonalDataTypeUuid, organizationUuid);
        }
    }
}