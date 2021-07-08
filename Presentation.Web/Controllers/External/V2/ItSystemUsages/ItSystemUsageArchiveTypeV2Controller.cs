using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Presentation.Web.Models.External.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    [RoutePrefix("api/v2/it-system-usage-archive-types")]
    [DenyRightsHoldersAccess]
    public class ItSystemUsageArchiveTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, ArchiveType>
    {
        public ItSystemUsageArchiveTypeV2Controller(IOptionsApplicationService<ItSystemUsage, ArchiveType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage archive option types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the archivetype availability</param>
        /// <returns>A list of available It-System Usage archive option types</returns>
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
        /// Returns requested It-System Usage archive option type
        /// </summary>
        /// <param name="archiveTypeUuid">archive type identifier</param>
        /// <param name="organizationUuid">organization context for the archive type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the archive type is available in the organization</returns>
        [HttpGet]
        [Route("{archiveTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid archiveTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(archiveTypeUuid, organizationUuid);
        }
    }
}