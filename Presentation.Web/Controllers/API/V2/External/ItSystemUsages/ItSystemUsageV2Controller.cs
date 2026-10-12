using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
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
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Helpers;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Presentation.Web.Models.API.V2.SharedProperties;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    /// <summary>
    /// API for the local registrations related to it-systems in KITOS
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// </summary>
    [Route("api/v2/it-system-usages")]
    public class ItSystemUsageV2Controller(
        IItSystemUsageService itSystemUsageService,
        IItSystemUsageResponseMapper responseMapper,
        IItSystemUsageWriteService writeService,
        IItSystemUsageWriteModelMapper writeModelMapper,
        IResourcePermissionsResponseMapper permissionsResponseMapper,
        IItsystemUsageRelationsService systemRelationsService,
        IExternalReferenceResponseMapper externalReferenceResponseMapper,
        IItSystemUsageArchiveResponseMapper archiveResponseMapper)
        : ExternalBaseController
    {
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
        /// <param name="paginationQuery">Pagination query parameters</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<ItSystemUsageResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetItSystemUsages(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUsageUuid = null,
            [NonEmptyGuid] Guid? relatedToContractUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            string? systemNameContent = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromQuery] BoundedPaginationQuery? paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return itSystemUsageService
                .ExecuteItSystemUsagesQuery(organizationUuid, relatedToSystemUuid, relatedToSystemUsageUuid, relatedToContractUuid, systemUuid, systemNameContent, changedSinceGtEq, orderByProperty, paginationQuery)
                .Select(responseMapper.MapSystemUsageDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the system usage entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}")]
        [ApiResponse(typeof(ItSystemUsageResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return itSystemUsageService
                .GetItSystemUsageByUuidAndAuthorizeRead(systemUsageUuid)
                .Select(responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the system usage entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/permissions")]
        [ApiResponse(typeof(ResourcePermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItSystemUsagePermissions([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return itSystemUsageService
                .GetPermissions(systemUsageUuid)
                .Select(permissionsResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the permissions of the authenticated client for the IT-System usage resources collection in the context of an organization (IT-System usage permissions in a specific Organization)
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns></returns>
        [HttpGet]
        [Route("permissions")]
        [ApiResponse(typeof(ResourceCollectionPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItSystemUsageCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return itSystemUsageService
                .GetCollectionPermissions(organizationUuid)
                .Select(permissionsResponseMapper.Map)
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
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
                .Delete(systemUsageUuid)
                .Match(FromOperationError, () => StatusCode((int)HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Creates an IT-System usage
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ApiResponse(typeof(ItSystemUsageResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostItSystemUsage([FromBody] CreateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
                .Create(new SystemUsageCreationParameters(request.SystemUuid, request.OrganizationUuid, writeModelMapper.FromPOST(request)))
                .Select(responseMapper.MapSystemUsageDTO)
                .Match(MapSystemCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Perform a full update of an existing system usage.
        /// Note: PUT expects a full version of the system usage. For partial updates, please use PATCH.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}")]
        [ApiResponse(typeof(ItSystemUsageResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutSystemUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] UpdateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = writeModelMapper.FromPUT(request);

            return writeService
                .Update(systemUsageUuid, updateParameters)
                .Select(responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Allows partial updates of an existing system usage
        /// NOTE:At the root level, defined sections will be mapped as changes e.g. {General: null} will reset the entire "General" section.
        /// If the section is not provided in the json, the omitted section will remain unchanged.
        /// At the moment we only manage PATCH at the root level so all levels below that must be provided in it's entirety.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{systemUsageUuid}")]
        [ApiResponse(typeof(ItSystemUsageResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PatchSystemUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] UpdateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updateParameters = writeModelMapper.FromPATCH(request);

            return writeService
                .Update(systemUsageUuid, updateParameters)
                .Select(responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Get all system relations TO the system usage FROM another system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/incoming-system-relations")]
        [ApiResponse(typeof(IEnumerable<IncomingSystemRelationResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetIncomingSystemRelations([NonEmptyGuid] Guid systemUsageUuid)
        {
            return itSystemUsageService
                .GetItSystemUsageByUuidAndAuthorizeRead(systemUsageUuid)
                .Bind(usage => systemRelationsService.GetRelationsTo(usage.Id))
                .Select(relations => relations.Select(relation => responseMapper.MapIncomingSystemRelationDTO(relation)).ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Add role assignment to the it-system usage
        /// Constraint: Duplicates are not allowed (existing assignment of the same user/role)
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{systemUsageUuid}/roles/add")]
        [ApiResponse(typeof(ItSystemUsageResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PatchAddRoleAssignment([NonEmptyGuid] Guid systemUsageUuid, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
                .AddRole(systemUsageUuid, request.ToUserRolePair())
                .Select(responseMapper.MapSystemUsageDTO)
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
        [ApiResponse(typeof(ItSystemUsageResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PatchRemoveRoleAssignment([NonEmptyGuid] Guid systemUsageUuid, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
                .RemoveRole(systemUsageUuid, request.ToUserRolePair())
                .Select(responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates a system relation FROM the system usage to another
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/system-relations")]
        [ApiResponse(typeof(OutgoingSystemRelationResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [FromBody][Required] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var systemRelationParameters = writeModelMapper.MapRelation(request);

            return writeService
                .CreateSystemRelation(systemUsageUuid, systemRelationParameters)
                .Select(responseMapper.MapOutgoingSystemRelationDTO)
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
        [ApiResponse(typeof(OutgoingSystemRelationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return itSystemUsageService
                .GetItSystemUsageByUuidAndAuthorizeRead(systemUsageUuid)
                .Bind(usage =>
                    usage.GetUsageRelation(systemRelationUuid)
                        .Match<Result<SystemRelation, OperationError>>
                        (
                        systemRelation => systemRelation,
                        () => new OperationError("Relation not found on system usage", OperationFailure.NotFound))
                    )
                .Select(responseMapper.MapOutgoingSystemRelationDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Updates the system relation FROM the system usage to another
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [ApiResponse(typeof(OutgoingSystemRelationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid, [FromBody] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var systemRelationParameters = writeModelMapper.MapRelation(request);

            return writeService
                .UpdateSystemRelation(systemUsageUuid, systemRelationUuid, systemRelationParameters)
                .Select(responseMapper.MapOutgoingSystemRelationDTO)
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
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
                .DeleteSystemRelation(systemUsageUuid, systemRelationUuid)
                .Match(FromOperationError, () => StatusCode((int)HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Creates an external reference for the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/external-references")]
        [ApiResponse(typeof(ExternalReferenceDataResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostExternalReference([NonEmptyGuid] Guid systemUsageUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = writeModelMapper.MapExternalReference(dto);

            return writeService
                .AddExternalReference(systemUsageUuid, properties)
                .Select(externalReferenceResponseMapper.MapExternalReference)
                .Match(reference => Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{systemUsageUuid}/external-references/{reference.Uuid}", reference), FromOperationError);
        }

        /// <summary>
        /// Updates a system usage external reference
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/external-references/{externalReferenceUuid}")]
        [ApiResponse(typeof(ExternalReferenceDataResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutExternalReference([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid externalReferenceUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = writeModelMapper.MapExternalReference(dto);

            return writeService
                .UpdateExternalReference(systemUsageUuid, externalReferenceUuid, properties)
                .Select(externalReferenceResponseMapper.MapExternalReference)
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
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteExternalReference([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid externalReferenceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
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
        [ApiResponse(typeof(JournalPeriodResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [FromBody][Required] JournalPeriodDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = writeModelMapper.MapJournalPeriodProperties(request);

            return writeService
                .CreateJournalPeriod(systemUsageUuid, parameters)
                .Select(responseMapper.MapJournalPeriodResponseDto)
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
        [ApiResponse(typeof(JournalPeriodResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid journalPeriodUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return itSystemUsageService
                .GetItSystemUsageByUuidAndAuthorizeRead(systemUsageUuid)
                .Bind(usage =>
                    usage.GetArchivePeriod(journalPeriodUuid)
                        .Match<Result<ArchivePeriod, OperationError>>
                        (
                        period => period,
                        () => new OperationError("Journal period not found on system usage", OperationFailure.NotFound))
                    )
                .Select(responseMapper.MapJournalPeriodResponseDto)
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
        [ApiResponse(typeof(JournalPeriodResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid journalPeriodUuid, [FromBody] JournalPeriodDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = writeModelMapper.MapJournalPeriodProperties(request);

            return writeService
                .UpdateJournalPeriod(systemUsageUuid, journalPeriodUuid, parameters)
                .Select(responseMapper.MapJournalPeriodResponseDto)
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
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteJournalPeriod([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid journalPeriodUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return writeService
                .DeleteJournalPeriod(systemUsageUuid, journalPeriodUuid)
                .Match(_ => NoContent(), FromOperationError);
        }

        /// <summary>
        /// Archives the specified ItSystemUsage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/archive")]
        [ApiResponse(typeof(ItSystemUsageArchiveResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult ArchiveItSystemUsage(
            [NonEmptyGuid] Guid systemUsageUuid,
            [FromBody] CreateItSystemUsageArchiveRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var parameters = writeModelMapper.MapArchiveParameters(request);
            return writeService.Archive(systemUsageUuid, parameters).Match(
                archive => Ok(archiveResponseMapper.ToResponseDTO(archive)),
                FromOperationError);
        }

        private IActionResult MapSystemCreatedResponse(ItSystemUsageResponseDTO dto)
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
        private IActionResult CreateCreatedResourcePath<T>(Guid systemUsageUuid, string resourceUrlSegment, T resourceDTO) where T : IHasUuidExternal
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{systemUsageUuid}/{resourceUrlSegment}/{resourceDTO.Uuid}", resourceDTO);
        }
    }
}


