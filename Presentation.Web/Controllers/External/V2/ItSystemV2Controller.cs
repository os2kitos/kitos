using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class ItSystemV2Controller : ExternalBaseController
    {
        private readonly IItSystemService _itSystemService;
        private readonly IRightsHoldersService _rightsHoldersService;

        public ItSystemV2Controller(IItSystemService itSystemService, IRightsHoldersService rightsHoldersService)
        {
            _itSystemService = itSystemService;
            _rightsHoldersService = rightsHoldersService;
        }

        /// <summary>
        /// Returns all IT-Systems available to the current user
        /// </summary>
        /// <param name="rightsHolderUuid">Rightsholder UUID filter</param>
        /// <param name="businessTypeUuid">Business type UUID filter</param>
        /// <param name="kleNumber">KLE number filter ("NN.NN.NN" format)</param>
        /// <param name="kleUuid">KLE UUID number filter</param>
        /// <param name="numberOfUsers">Greater than or equal to number of users filter</param>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            Guid? rightsHolderUuid = null,
            Guid? businessTypeUuid = null,
            string kleNumber = null,
            Guid? kleUuid = null,
            int? numberOfUsers = null,
            [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItSystem>>();

            if (rightsHolderUuid.HasValue)
                refinements.Add(new QueryByRightsHolderUuid(rightsHolderUuid.Value));

            if (businessTypeUuid.HasValue)
                refinements.Add(new QueryByBusinessType(businessTypeUuid.Value));

            if (kleNumber != null || kleUuid.HasValue)
                refinements.Add(new QueryByTaskRef(kleNumber, kleUuid));

            if (numberOfUsers.HasValue)
                refinements.Add(new QueryByNumberOfUsages(numberOfUsers.Value));

            return _itSystemService.GetAvailableSystems(refinements.ToArray())
                .OrderBy(x => x.Id)
                .Page(paginationQuery)
                .ToList()
                .Select(ToSystemResponseDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystem(Guid uuid)
        {
            if (uuid == Guid.Empty)
                return BadRequest(nameof(uuid) + " must be a non-empty guid");

            return _itSystemService
                .GetSystem(uuid)
                .Select(ToSystemResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns IT-Systems for which the current user has rights holders access
        /// </summary>
        /// <param name="rightsHolderUuid">Optional filtering if a user is rights holder in multiple organizations and wishes to scope the request to a single one</param>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("rightsholder/it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RightsHolderItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemsByRightsHoldersAccess(Guid? rightsHolderUuid = null, [FromUri] StandardPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHoldersService
                .GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(rightsHolderUuid)
                .Select(itSystems => itSystems
                    .OrderBy(system => system.Id)
                    .Page(paginationQuery)
                    .ToList()
                    .Select(ToSystemInformationResponseDTO)
                    .ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemByRightsHoldersAccess(Guid uuid)
        {
            if (uuid == Guid.Empty)
                return BadRequest(nameof(uuid) + " must be a non-empty guid");

            return _rightsHoldersService
                .GetSystemAsRightsHolder(uuid)
                .Select(ToSystemInformationResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates a new IT-System based on given input values
        /// </summary>
        /// <param name="itSystemRequestDTO">A collection of specific IT-System values</param>
        /// <returns>Location header is set to uri for newly created IT-System</returns>
        [HttpPost]
        [Route("rightsholder/it-systems")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PostItSystem([FromBody] ItSystemRequestDTO itSystemRequestDTO)
        {
            //TODO: Implement
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{itSystemRequestDTO.Uuid}", new RightsHolderItSystemResponseDTO());
        }

        /// <summary>
        /// Sets IT-System values
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>The updated IT-System</returns>
        [HttpPut]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItSystem(Guid uuid, [FromBody] ItSystemRequestDTO itSystemRequestDTO)
        {
            return Ok(new RightsHolderItSystemResponseDTO());
        }

        /// <summary>
        /// Deactivates an IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <param name="deactivationReasonDTO">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteItSystem(Guid uuid, [FromBody] DeactivationReasonRequestDTO deactivationReasonDTO)
        {
            return Ok();
        }

        private static RightsHolderItSystemResponseDTO ToSystemInformationResponseDTO(ItSystem itSystem)
        {
            var dto = new RightsHolderItSystemResponseDTO();
            MapBaseInformation(itSystem, dto);
            return dto;
        }

        private static ItSystemResponseDTO ToSystemResponseDTO(ItSystem itSystem)
        {
            var dto = new ItSystemResponseDTO
            {
                UsingOrganizations = itSystem
                    .Usages
                    .Select(systemUsage => systemUsage.Organization)
                    .Select(organization => organization.MapOrganizationResponseDTO())
                    .ToList(),
                LastModified = itSystem.LastChanged,
                LastModifiedBy = itSystem.LastChangedByUser.Transform(user => user.MapIdentityNamePairDTO())
            };

            MapBaseInformation(itSystem, dto);

            return dto;
        }

        private static void MapBaseInformation<T>(ItSystem arg, T dto) where T : BaseItSystemResponseDTO
        {
            dto.Uuid = arg.Uuid;
            dto.Name = arg.Name;
            dto.RightsHolder = arg.BelongsTo?.Transform(organization => organization.MapOrganizationResponseDTO());
            dto.BusinessType = arg.BusinessType?.Transform(businessType => businessType.MapIdentityNamePairDTO());
            dto.Description = arg.Description;
            dto.CreatedBy = arg.ObjectOwner.MapIdentityNamePairDTO();
            dto.Created = arg.Created;
            dto.Deactivated = arg.Disabled;
            dto.FormerName = arg.PreviousName;
            dto.ParentSystem = arg.Parent?.Transform(parent => parent.MapIdentityNamePairDTO());
            dto.UrlReference = arg.Reference?.URL;
            dto.ExposedInterfaces = arg
                .ItInterfaceExhibits
                .Select(exhibit => exhibit.ItInterface)
                .ToList()
                .Select(x => x.MapIdentityNamePairDTO())
                .ToList();
            dto.RecommendedArchiveDutyResponse =
                new RecommendedArchiveDutyResponseDTO(arg.ArchiveDutyComment, arg.ArchiveDuty.ToDTOType());
            dto.KLE = arg
                .TaskRefs
                .Select(taskRef => new IdentityNamePairResponseDTO(taskRef.Uuid, taskRef.TaskKey))
                .ToList();
        }
    }
}