using Core.ApplicationServices.Project;
using Core.DomainModel.ItProject;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.External.V2.ItProjects
{
    /// <summary>
    /// API for the projects stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2")]
    public class ItProjectV2Controller : ExternalBaseController
    {
        private readonly IItProjectService _itProjectService;

        public ItProjectV2Controller(IItProjectService itProjectService)
        {
            _itProjectService = itProjectService;
        }

        /// <summary>
        /// Returns all IT-Projects available to the current user
        /// </summary>
        /// <param name="organizationUuid">Organization UUID filter</param>
        /// <param name="nameContent">Name content filter</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-projects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItProjects(
            [Required][NonEmptyGuid] Guid organizationUuid,
            string nameContent = null,
            [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItProject>>();

            refinements.Add(new QueryByOrganizationUuid<ItProject>(organizationUuid));

            if (!string.IsNullOrWhiteSpace(nameContent))
                refinements.Add(new QueryByPartOfName<ItProject>(nameContent));

            return _itProjectService
                .GetAvailableProjects(refinements.ToArray())
                .OrderBy(x => x.Id)
                .Page(paginationQuery)
                .ToList()
                .Select(x => x.MapIdentityNamePairDTO())
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("it-projects/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItProject([Required][NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itProjectService
                .GetProject(uuid)
                .Select(x => x.MapIdentityNamePairDTO())
                .Match(Ok, FromOperationError);
        }
    }
}