﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Presentation.Web.Controllers.API.V2.Common.Helpers;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Shared;
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
        private readonly IItSystemWriteModelMapper _writeModelMapper;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IExternalReferenceResponseMapper _referenceResponseMapper;

        public ItSystemV2Controller(IItSystemService itSystemService,
            IRightsHolderSystemService rightsHolderSystemService,
            IItSystemWriteModelMapper writeModelMapper,
            IEntityIdentityResolver entityIdentityResolver,
            IExternalReferenceResponseMapper referenceResponseMapper)
        {
            _itSystemService = itSystemService;
            _rightsHolderSystemService = rightsHolderSystemService;
            _writeModelMapper = writeModelMapper;
            _entityIdentityResolver = entityIdentityResolver;
            _referenceResponseMapper = referenceResponseMapper;
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
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
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
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService
                .ExecuteItSystemsQuery(rightsHolderUuid, businessTypeUuid, kleNumber, kleUuid, numberOfUsers, includeDeactivated, changedSinceGtEq, paginationQuery)
                .Select(ToSystemResponseDTO)
                .Transform(Ok);
        }


        /// <summary>
        /// Create a new IT-System master data entity
        /// NOTE: This is for master data only. Local usages extend this with local data, and are managed through the it-system-usage resource
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("it-systems")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostItSystem([FromBody] CreateItSystemRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromPOST(request);
            return _rightsHolderSystemService
                .CreateNewSystem(request.OrganizationUuid, parameters)
                .Select(ToSystemResponseDTO)
                .Match(MapSystemCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Update an existing it-system
        /// NOTE: This is for master data only. Local usages extend this with local data, and are managed through the it-system-usage resource
        /// </summary>
        /// <returns></returns>
        [HttpPatch]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostItSystem([NonEmptyGuid] Guid uuid, [FromBody] UpdateItSystemRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromPATCH(request);
            return _rightsHolderSystemService
                .Update(uuid, parameters)
                .Select(ToSystemResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// DELETE an existing it-system
        /// NOTE: This is for master data only. Local usages extend this with local data, and are managed through the it-system-usage resource
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("it-systems/{uuid}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteItSystem([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderSystemService
                .Delete(uuid)
                .Match(NoContent, FromOperationError);
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
        /// Returns hierarchy for the specified IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems/{uuid}/hierarchy")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegistrationHierarchyNodeResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetHierarchy([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _entityIdentityResolver.ResolveDbId<ItSystem>(uuid)
                .Match
                (
                    id => _itSystemService.GetHierarchy(id),
                    () => new OperationError($"System with uuid: {uuid} was not found", OperationFailure.NotFound)
                )
                .Select(RegistrationHierarchyNodeMapper.MapHierarchyToDtos)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns IT-Systems for which the current user has rights holders access
        /// </summary>
        /// <param name="rightsHolderUuid">Optional filtering if a user is rights holder in multiple organizations and wishes to scope the request to a single one</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-interfaces</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <returns></returns>
        [HttpGet]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RightsHolderItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemsByRightsHoldersAccess(
            [NonEmptyGuid] Guid? rightsHolderUuid = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItSystem>>();

            if (includeDeactivated != true)
                refinements.Add(new QueryByEnabledEntitiesOnly<ItSystem>());

            if (changedSinceGtEq.HasValue)
                refinements.Add(new QueryByChangedSinceGtEq<ItSystem>(changedSinceGtEq.Value));

            return _rightsHolderSystemService
                .GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(refinements, rightsHolderUuid)
                .Select(itSystems => itSystems
                    .OrderByDefaultConventions(changedSinceGtEq.HasValue)
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
        public IHttpActionResult PostItSystemAsRightsHolder([FromBody] RightsHolderFullItSystemRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromRightsHolderPOST(request);

            return _rightsHolderSystemService
                .CreateNewSystemAsRightsHolder(request.RightsHolderUuid, parameters)
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
        public IHttpActionResult PutItSystemAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderFullItSystemRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromRightsHolderPUT(request);

            return _rightsHolderSystemService
                .UpdateAsRightsHolder(uuid, parameters)
                .Select(ToRightsHolderResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Partially updates an existing it-system using json merge patch semantics (RFC7396)
        /// NOTE: Only active systems can be modified.
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>The updated IT-System</returns>
        [HttpPatch]
        [AllowRightsHoldersAccess]
        [Route("rightsholder/it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RightsHolderItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PatchItSystemAsRightsHolder([NonEmptyGuid] Guid uuid, [FromBody] RightsHolderUpdateSystemPropertiesRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromRightsHolderPATCH(request);

            return _rightsHolderSystemService
                .UpdateAsRightsHolder(uuid, parameters)
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
                .DeactivateAsRightsHolder(uuid, request.DeactivationReason)
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
                LastModifiedBy = itSystem.LastChangedByUser.Transform(user => user.MapIdentityNamePairDTO()),
                Scope = itSystem.AccessModifier.ToChoice()
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
            dto.ExternalReferences = _referenceResponseMapper.MapExternalReferences(arg.ExternalReferences).ToList();
            dto.RecommendedArchiveDuty = new RecommendedArchiveDutyResponseDTO(arg.ArchiveDutyComment, arg.ArchiveDuty?.ToChoice() ?? RecommendedArchiveDutyChoice.Undecided);
            dto.KLE = arg
                .TaskRefs
                .Select(taskRef => taskRef.MapIdentityNamePairDTO())
                .ToList();
        }
        private CreatedNegotiatedContentResult<T> MapSystemCreatedResponse<T>(T dto) where T : IHasUuidExternal
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}