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
    [RoutePrefix("api/v2/it-system-usage-archive-types")]
    public class ItSystemUsageArchiveTypeV2Controller : BaseOptionTypeV2Controller<ItSystemUsage, ArchiveType>
    {
        public ItSystemUsageArchiveTypeV2Controller(IOptionsApplicationService<ItSystemUsage, ArchiveType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage archive option types
        /// </summary>
        /// <param name="organizationUuid">organization context for the archivetype availability</param>
        /// <returns>A list of available It-System Usage archive option types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemUsageDataClassifications(Guid organizationUuid, [FromUri] StandardPaginationQuery pagination = null)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AvailableNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemUsageDataClassification(Guid archiveTypeUuid, Guid organizationUuid)
        {
            return GetSingle(archiveTypeUuid, organizationUuid);
        }
    }
}