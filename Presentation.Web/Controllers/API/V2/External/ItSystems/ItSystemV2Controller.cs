using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.ItSystem;
using Infrastructure.Services.Types;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Request.System;
using Presentation.Web.Models.API.V2.Response.System;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems
{
    /// <summary>
    /// API for the system product master data (not local organization usage registrations) stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2")]
    public class ItSystemV2Controller : ExternalBaseController
    {
        private readonly IItSystemService _itSystemService;
        private readonly IRightsHolderSystemService _rightsHolderSystemService;
        private readonly IAuthorizationContext _authorizationContext;

        public ItSystemV2Controller(IItSystemService itSystemService, IRightsHolderSystemService rightsHolderSystemService, IAuthorizationContext authorizationContext)
        {
            _itSystemService = itSystemService;
            _rightsHolderSystemService = rightsHolderSystemService;
            _authorizationContext = authorizationContext;
        }

        /// <summary>
        /// Returns all IT-Systems available to the current user
        /// </summary>
        /// <param name="rightsHolderUuid">Rightsholder UUID filter</param>
        /// <param name="businessTypeUuid">Business type UUID filter</param>
        /// <param name="kleNumber">KLE number filter ("NN.NN.NN" format)</param>
        /// <param name="kleUuid">KLE UUID number filter</param>
        /// <param name="numberOfUsers">Greater than or equal to number of users filter</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            [NonEmptyGuid] Guid? rightsHolderUuid = null,
            [NonEmptyGuid] Guid? businessTypeUuid = null,
            string kleNumber = null,
            [NonEmptyGuid] Guid? kleUuid = null,
            int? numberOfUsers = null,
            bool includeDeactivated = false,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
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

            if (includeDeactivated == false)
                refinements.Add(new QueryByEnabledEntitiesOnly<ItSystem>());

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
        public IHttpActionResult GetItSystem([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService
                .GetSystem(uuid)
                .Select(ToSystemResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns IT-Systems for which the current user has rights holders access
        /// </summary>
        /// <param name="rightsHolderUuid">Optional filtering if a user is rights holder in multiple organizations and wishes to scope the request to a single one</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <returns></returns>
        [HttpGet]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RightsHolderItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemsByRightsHoldersAccess(
            [NonEmptyGuid] Guid? rightsHolderUuid = null,
            bool includeDeactivated = false,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItSystem>>();

            if (includeDeactivated == false)
                refinements.Add(new QueryByEnabledEntitiesOnly<ItSystem>());

            return _rightsHolderSystemService
                .GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(refinements, rightsHolderUuid)
                .Select(itSystems => itSystems
                    .OrderBy(system => system.Id)
                    .Page(paginationQuery)
                    .ToList()
                    .Select(ToRightsHolderResponseDTO)
                    .ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemByRightsHoldersAccess([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderSystemService
                .GetSystemAsRightsHolder(uuid)
                .Select(ToRightsHolderResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates a new IT-System based on given input values
        /// </summary>
        /// <param name="request">A collection of specific IT-System values</param>
        /// <returns>Location header is set to uri for newly created IT-System</returns>
        [HttpPost]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PostItSystemAsRightsHolder([FromBody] RightsHolderCreateItSystemRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = new RightsHolderSystemCreationParameters(
                request.Name,
                request.Uuid,
                request.ParentUuid,
                request.FormerName, request.Description, request.UrlReference, request.BusinessTypeUuid,
                request.KLENumbers ?? new string[0], request.KLEUuids ?? new Guid[0]);

            return _rightsHolderSystemService
                .CreateNewSystem(request.RightsHolderUuid, parameters)
                .Select(ToRightsHolderResponseDTO)
                .Match(MapSystemCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Sets IT-System values
        /// If a property value is not provided, KITOS will fallback to the default value of the type and that will be written to the it-system so remember to define all data specified in the request DTO want them to have a value after the request.
        /// Required properties dictate the minimum value set accepted by KITOS.
		/// NOTE: Only active systems can be modified.
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>The updated IT-System</returns>
        [HttpPut]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItSystemAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderWritableITSystemPropertiesDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = new RightsHolderSystemUpdateParameters(request.Name, request.ParentUuid, request.FormerName,
                request.Description, request.UrlReference, request.BusinessTypeUuid,
                request.KLENumbers ?? Array.Empty<string>(), request.KLEUuids ?? Array.Empty<Guid>());

            return _rightsHolderSystemService
                .Update(uuid, parameters)
                .Select(ToRightsHolderResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deactivates an IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <param name="request">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeactivateSystemAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] DeactivationReasonRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderSystemService
                .Deactivate(uuid, request.DeactivationReason)
                .Select(ToRightsHolderResponseDTO)
                .Match(_ => StatusCode(HttpStatusCode.NoContent), FromOperationError);
        }

        private RightsHolderItSystemResponseDTO ToRightsHolderResponseDTO(ItSystem itSystem)
        {
            var dto = new RightsHolderItSystemResponseDTO();
            MapBaseInformation(itSystem, dto);
            return dto;
        }

        private ItSystemResponseDTO ToSystemResponseDTO(ItSystem itSystem)
        {
            var dto = new ItSystemResponseDTO
            {
                UsingOrganizations = itSystem
                    .Usages
                    .Select(systemUsage => systemUsage.Organization)
                    .Select(organization => organization.MapShallowOrganizationResponseDTO())
                    .ToList(),
                LastModified = itSystem.LastChanged,
                LastModifiedBy = itSystem.LastChangedByUser.Transform(user => user.MapIdentityNamePairDTO())
            };

            MapBaseInformation(itSystem, dto);

            return dto;
        }

        private void MapBaseInformation<T>(ItSystem arg, T dto) where T : BaseItSystemResponseDTO
        {
            dto.Uuid = arg.Uuid;
            dto.Name = arg.Name;
            dto.RightsHolder = arg.BelongsTo?.Transform(organization => organization.MapShallowOrganizationResponseDTO());
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
                .Where(_authorizationContext.AllowReads)// Only accessible interfaces may be referenced here
                .Select(x => x.MapIdentityNamePairDTO())
                .ToList();
            dto.RecommendedArchiveDuty =
                new RecommendedArchiveDutyResponseDTO(arg.ArchiveDutyComment, arg.ArchiveDuty.ToDTOType());
            dto.KLE = arg
                .TaskRefs
                .Select(taskRef => taskRef.MapIdentityNamePairDTO())
                .ToList();
        }
        private CreatedNegotiatedContentResult<RightsHolderItSystemResponseDTO> MapSystemCreatedResponse(RightsHolderItSystemResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}