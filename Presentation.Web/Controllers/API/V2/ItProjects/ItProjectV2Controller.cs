using Core.ApplicationServices.Project;
using Core.DomainModel.ItProject;
using Core.DomainServices.Queries;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Response;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.External.ItProjects
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
        /// Returns all IT-Projects in the requested organization available to the user
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
            [NonEmptyGuid] Guid organizationUuid,
            string nameContent = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItProject>>();

            if (!string.IsNullOrWhiteSpace(nameContent))
                refinements.Add(new QueryByPartOfName<ItProject>(nameContent));

            return _itProjectService
                .GetProjectsInOrganization(organizationUuid, refinements.ToArray())
                .Select(x => x.OrderBy(project => project.Id))
                .Select(x => x.Page(paginationQuery))
                .Select(x => x.ToList().Select(x => x.MapIdentityNamePairDTO()))
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-Project
        /// </summary>
        /// <param name="uuid">Specific IT-Project UUID</param>
        /// <returns>Specific data related to the IT-Project</returns>
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