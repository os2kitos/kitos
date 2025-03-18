using System;
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
using Swashbuckle.Swagger.Annotations;
using Core.ApplicationServices.System.Write;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Response.Shared;
using System.ComponentModel.DataAnnotations;
using System.Web.ModelBinding;
using Presentation.Web.Models.API.V2.Types.Shared;

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
        private readonly IItSystemWriteService _writeService;
        private readonly IItSystemResponseMapper _systemResponseMapper;
        private readonly IExternalReferenceResponseMapper _referenceResponseMapper;
        private readonly IResourcePermissionsResponseMapper _permissionResponseMapper;

        public ItSystemV2Controller(IItSystemService itSystemService,
            IRightsHolderSystemService rightsHolderSystemService,
            IItSystemWriteModelMapper writeModelMapper,
            IEntityIdentityResolver entityIdentityResolver,
            IItSystemWriteService writeService,
            IItSystemResponseMapper systemResponseMapper,
            IExternalReferenceResponseMapper referenceResponseMapper,
            IResourcePermissionsResponseMapper permissionResponseMapper)
        {
            _itSystemService = itSystemService;
            _rightsHolderSystemService = rightsHolderSystemService;
            _writeModelMapper = writeModelMapper;
            _entityIdentityResolver = entityIdentityResolver;
            _writeService = writeService;
            _systemResponseMapper = systemResponseMapper;
            _referenceResponseMapper = referenceResponseMapper;
            _permissionResponseMapper = permissionResponseMapper;
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
        /// <param name="usedInOrganizationUuid">Filter by UUID of an organization which has taken the it-system into use through an it-system-usage resource</param>
        /// <param name="nameContains">Include only systems with a name that contains the content in the parameter</param>
        /// <param name="orderByProperty">Ordering property</param>
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
            [NonEmptyGuid] Guid? usedInOrganizationUuid = null,
            string nameContains = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService
                .ExecuteItSystemsQuery(rightsHolderUuid, businessTypeUuid, kleNumber, kleUuid, numberOfUsers, includeDeactivated, changedSinceGtEq, usedInOrganizationUuid, nameContains: nameContains, orderByProperty:orderByProperty,paginationQuery: paginationQuery)
                .Select(_systemResponseMapper.ToSystemResponseDTO)
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
            return _writeService
                .CreateNewSystem(request.OrganizationUuid, parameters)
                .Select(_systemResponseMapper.ToSystemResponseDTO)
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
        public IHttpActionResult PatchItSystem([NonEmptyGuid] Guid uuid, [FromBody] UpdateItSystemRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromPATCH(request);
            return _writeService
                .Update(uuid, parameters)
                .Select(_systemResponseMapper.ToSystemResponseDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// DELETE an existing it-system
        /// NOTE: This is for master data only. Local usages extend this with local data, and are managed through the it-system-usage resource
        /// Constraints:
        /// - All usages must be removed before deletion
        /// - All child systems must be removed
        /// - No interfaces may still be exposed on the it-system
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

            return _writeService
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
                .Select(_systemResponseMapper.ToSystemResponseDTO)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Returns hierarchy for the specified IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems/{uuid}/hierarchy")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemHierarchyNodeResponseDTO>))]
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
                    id => _itSystemService.GetCompleteHierarchy(id),
                    () => new OperationError($"System with uuid: {uuid} was not found", OperationFailure.NotFound)
                )
                .Select(RegistrationHierarchyNodeMapper.MapSystemHierarchyToDtos)
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
                    .OrderApiResultsByDefaultConventions(changedSinceGtEq.HasValue)
                    .Page(paginationQuery)
                    .ToList()
                    .Select(_systemResponseMapper.ToRightsHolderResponseDTO)
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
                .Select(_systemResponseMapper.ToRightsHolderResponseDTO)
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
                .Select(_systemResponseMapper.ToRightsHolderResponseDTO)
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
                .Select(_systemResponseMapper.ToRightsHolderResponseDTO)
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
                .Select(_systemResponseMapper.ToRightsHolderResponseDTO)
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
                .Select(_systemResponseMapper.ToRightsHolderResponseDTO)
                .Match(_ => StatusCode(HttpStatusCode.NoContent), FromOperationError);
        }

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific IT-System
        /// </summary>
        /// <param name="systemUuid">UUID of the system entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems/{systemUuid}/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemPermissions([NonEmptyGuid] Guid systemUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService
                .GetPermissions(systemUuid)
                .Select(_systemResponseMapper.MapPermissions)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Returns the permissions of the authenticated client for the IT-System resources collection in the context of an organization (IT-System permissions in a specific Organization)
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourceCollectionPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService.GetCollectionPermissions(organizationUuid)
                .Select(_permissionResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Creates an external reference for the system
        /// </summary>
        /// <param name="systemUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("it-systems/{systemUuid}/external-references")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ExternalReferenceDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostExternalReference([NonEmptyGuid] Guid systemUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .AddExternalReference(systemUuid, properties)
                .Select(_referenceResponseMapper.MapExternalReference)
                .Match(reference => Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{systemUuid}/external-references/{reference.Uuid}", reference), FromOperationError);
        }

        /// <summary>
        /// Updates a system external reference
        /// </summary>
        /// <param name="systemUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("it-systems/{systemUuid}/external-references/{externalReferenceUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalReferenceDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutExternalReference([NonEmptyGuid] Guid systemUuid, [NonEmptyGuid] Guid externalReferenceUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .UpdateExternalReference(systemUuid, externalReferenceUuid, properties)
                .Select(_referenceResponseMapper.MapExternalReference)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a system external reference
        /// </summary>
        /// <param name="systemUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("it-systems/{systemUuid}/external-references/{externalReferenceUuid}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteExternalReference([NonEmptyGuid] Guid systemUuid, [NonEmptyGuid] Guid externalReferenceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteExternalReference(systemUuid, externalReferenceUuid)
                .Match(_ => NoContent(), FromOperationError);
        }

        private CreatedNegotiatedContentResult<T> MapSystemCreatedResponse<T>(T dto) where T : IHasUuidExternal
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}