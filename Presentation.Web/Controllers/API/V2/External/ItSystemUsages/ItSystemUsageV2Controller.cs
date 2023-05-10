using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Helpers;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    /// <summary>
    /// API for the local registrations related to it-systems in KITOS
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// </summary>
    [RoutePrefix("api/v2/it-system-usages")]
    public class ItSystemUsageV2Controller : ExternalBaseController
    {
        private readonly IItSystemUsageService _itSystemUsageService;
        private readonly IItSystemUsageResponseMapper _responseMapper;
        private readonly IItSystemUsageWriteService _writeService;
        private readonly IItSystemUsageWriteModelMapper _writeModelMapper;
        private readonly IResourcePermissionsResponseMapper _permissionsResponseMapper;
        private readonly IItsystemUsageRelationsService _systemRelationsService;
        private readonly IExternalReferenceResponseMapper _externalReferenceResponseMapper;

        public ItSystemUsageV2Controller(
            IItSystemUsageService itSystemUsageService,
            IItSystemUsageResponseMapper responseMapper,
            IItSystemUsageWriteService writeService,
            IItSystemUsageWriteModelMapper writeModelMapper,
            IResourcePermissionsResponseMapper permissionsResponseMapper,
            IItsystemUsageRelationsService systemRelationsService,
            IExternalReferenceResponseMapper externalReferenceResponseMapper)
        {
            _itSystemUsageService = itSystemUsageService;
            _responseMapper = responseMapper;
            _writeService = writeService;
            _writeModelMapper = writeModelMapper;
            _permissionsResponseMapper = permissionsResponseMapper;
            _systemRelationsService = systemRelationsService;
            _externalReferenceResponseMapper = externalReferenceResponseMapper;
        }

        /// <summary>
        /// Returns all IT-System usages available to the current user
        /// </summary>
        /// <param name="organizationUuid">Query usages within a specific organization</param>
        /// <param name="relatedToSystemUuid">Query by systems with outgoing relations related to another system</param>
        /// <param name="relatedToSystemUsageUuid">Query by system usages with outgoing relations to a specific system usage (more narrow search than using system id)</param>
        /// <param name="relatedToContractUuid">Query by contracts which are part of a system relation</param>
        /// <param name="systemUuid">Query usages of a specific system</param>
        /// <param name="systemNameContent">Query usages based on system name</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <param name="orderByProperty">Ordering property</param>
        /// <returns></returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemUsageResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetItSystemUsages(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUsageUuid = null,
            [NonEmptyGuid] Guid? relatedToContractUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            string systemNameContent = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .ExecuteItSystemUsagesQuery(organizationUuid, relatedToSystemUuid, relatedToSystemUsageUuid, relatedToContractUuid, systemUuid, systemNameContent, changedSinceGtEq, orderByProperty, paginationQuery)
                .Select(_responseMapper.MapSystemUsageDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the system usage entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the system usage entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemUsagePermissions([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .GetPermissions(systemUsageUuid)
                .Select(_permissionsResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the permissions of the authenticated client for the IT-System usage resources collection in the context of an organization (IT-System usage permissions in a specific Organization)
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns></returns>
        [HttpGet]
        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourceCollectionPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItSystemUsageCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .GetCollectionPermissions(organizationUuid)
                .Select(_permissionsResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a system usage.
        /// NOTE: this action also clears any incoming relation e.g. relations from other system usages, contracts or data processing registrations.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Delete(systemUsageUuid)
                .Match(FromOperationError, () => StatusCode(HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Creates an IT-System usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict, description: "Another system usage has already been created for the system within the specified organization")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostItSystemUsage([FromBody] CreateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Create(new SystemUsageCreationParameters(request.SystemUuid, request.OrganizationUuid, _writeModelMapper.FromPOST(request)))
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(MapSystemCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Perform a full update of an existing system usage.
        /// Note: PUT expects a full version of the system usage. For partial updates, please use PATCH.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] UpdateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = _writeModelMapper.FromPUT(request);

            return _writeService
                .Update(systemUsageUuid, updateParameters)
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Allows partial updates of an existing system usage
        /// NOTE:At the root level, defined sections will be mapped as changes e.g. {General: null} will reset the entire "General" section.
        /// If the section is not provided in the json, the omitted section will remain unchanged.
        /// At the moment we only manage PATCH at the root level so all levels below that must be provided in it's entirety.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PatchSystemUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] UpdateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = _writeModelMapper.FromPATCH(request);

            return _writeService
                .Update(systemUsageUuid, updateParameters)
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Get all system relations TO the system usage FROM another system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/incoming-system-relations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IncomingSystemRelationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetIncomingSystemRelations([NonEmptyGuid] Guid systemUsageUuid)
        {
            return _itSystemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Bind(usage => _systemRelationsService.GetRelationsTo(usage.Id))
                .Select(relations => relations.Select(relation => _responseMapper.MapIncomingSystemRelationDTO(relation)).ToList())
                .Match(Ok, FromOperationError);
        }

        /// Add role assignment to the it-system usage
        /// Constraint: Duplicates are not allowed (existing assignment of the same user/role)
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{systemUsageUuid}/roles/add")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "If duplicate is detected")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PatchAddRoleAssignment([NonEmptyGuid] Guid systemUsageUuid, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .AddRole(systemUsageUuid, request.ToUserRolePair())
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Remove an existing role assignment to the it-system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{systemUsageUuid}/roles/remove")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PatchRemoveRoleAssignment([NonEmptyGuid] Guid systemUsageUuid, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .RemoveRole(systemUsageUuid, request.ToUserRolePair())
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates a system relation FROM the system usage to another
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/system-relations")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(OutgoingSystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [FromBody][Required] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var systemRelationParameters = _writeModelMapper.MapRelation(request);

            return _writeService
                .CreateSystemRelation(systemUsageUuid, systemRelationParameters)
                .Select(_responseMapper.MapOutgoingSystemRelationDTO)
                .Match(relationDTO => CreateCreatedResourcePath(systemUsageUuid, "system-relations", relationDTO), FromOperationError);
        }

        /// <summary>
        /// Gets a specific relation FROM the system usage to another
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OutgoingSystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Bind(usage =>
                    usage.GetUsageRelation(systemRelationUuid)
                        .Match<Result<SystemRelation, OperationError>>
                        (
                        systemRelation => systemRelation,
                        () => new OperationError("Relation not found on system usage", OperationFailure.NotFound))
                    )
                .Select(_responseMapper.MapOutgoingSystemRelationDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Updates the system relation FROM the system usage to another
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OutgoingSystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid, [FromBody] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var systemRelationParameters = _writeModelMapper.MapRelation(request);

            return _writeService
                .UpdateSystemRelation(systemUsageUuid, systemRelationUuid, systemRelationParameters)
                .Select(_responseMapper.MapOutgoingSystemRelationDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a system relation FROM the system usage to another
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteSystemRelation(systemUsageUuid, systemRelationUuid)
                .Match(FromOperationError, () => StatusCode(HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Creates an external reference for the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/external-references")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ExternalReferenceDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostExternalReference([NonEmptyGuid] Guid systemUsageUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .AddExternalReference(systemUsageUuid, properties)
                .Select(_externalReferenceResponseMapper.MapExternalReference)
                .Match(reference => Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{systemUsageUuid}/external-references/{reference.Uuid}", reference), FromOperationError);
        }

        /// <summary>
        /// Updates a system usage external reference
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/external-references/{externalReferenceUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalReferenceDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutExternalReference([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid externalReferenceUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .UpdateExternalReference(systemUsageUuid, externalReferenceUuid, properties)
                .Select(_externalReferenceResponseMapper.MapExternalReference)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a system usage external reference
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}/external-references/{externalReferenceUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteExternalReference([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid externalReferenceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteExternalReference(systemUsageUuid, externalReferenceUuid)
                .Match(_ => NoContent(), FromOperationError);
        }

        /// <summary>
        /// Add a single journal period to the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/journal-periods")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(JournalPeriodResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [FromBody][Required] JournalPeriodDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.MapJournalPeriodProperties(request);

            return _writeService
                .CreateJournalPeriod(systemUsageUuid, parameters)
                .Select(_responseMapper.MapJournalPeriodResponseDto)
                .Match(period => CreateCreatedResourcePath(systemUsageUuid, "journal-periods", period), FromOperationError);
        }

        /// <summary>
        /// Get a specific journal period
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="journalPeriodUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/journal-periods/{journalPeriodUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(JournalPeriodResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid journalPeriodUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Bind(usage =>
                    usage.GetArchivePeriod(journalPeriodUuid)
                        .Match<Result<ArchivePeriod, OperationError>>
                        (
                        period => period,
                        () => new OperationError("Journal period not found on system usage", OperationFailure.NotFound))
                    )
                .Select(_responseMapper.MapJournalPeriodResponseDto)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Perform a FULL update of an existing journal period
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="journalPeriodUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/journal-periods/{journalPeriodUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(JournalPeriodResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid journalPeriodUuid, [FromBody] JournalPeriodDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.MapJournalPeriodProperties(request);

            return _writeService
                .UpdateJournalPeriod(systemUsageUuid, journalPeriodUuid, parameters)
                .Select(_responseMapper.MapJournalPeriodResponseDto)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Delete an existing journal period
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="journalPeriodUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}/journal-periods/{journalPeriodUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid journalPeriodUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteJournalPeriod(systemUsageUuid, journalPeriodUuid)
                .Match(_ => NoContent(), FromOperationError);
        }

        private CreatedNegotiatedContentResult<ItSystemUsageResponseDTO> MapSystemCreatedResponse(ItSystemUsageResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
        private CreatedNegotiatedContentResult<T> CreateCreatedResourcePath<T>(Guid systemUsageUuid, string resourceUrlSegment, T resourceDTO) where T : IHasUuidExternal
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{systemUsageUuid}/{resourceUrlSegment}/{resourceDTO.Uuid}", resourceDTO);
        }
    }
}