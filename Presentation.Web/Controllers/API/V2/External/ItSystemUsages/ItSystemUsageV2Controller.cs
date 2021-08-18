using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.SystemUsage;
using Infrastructure.Services.Types;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Swashbuckle.Swagger.Annotations;

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

        public ItSystemUsageV2Controller(IItSystemUsageService itSystemUsageService, IItSystemUsageResponseMapper responseMapper, IItSystemUsageWriteService writeService)
        {
            _itSystemUsageService = itSystemUsageService;
            _responseMapper = responseMapper;
            _writeService = writeService;
        }

        /// <summary>
        /// Returns all IT-System usages available to the current user
        /// </summary>
        /// <param name="organizationUuid">Query usages within a specific organization</param>
        /// <param name="relatedToSystemUuid">Query by systems with outgoing relations related to another system</param>
        /// <param name="relatedToSystemUsageUuid">Query by system usages with outgoing relations to a specific system usage (more narrow search than using system id)</param>
        /// <param name="systemUuid">Query usages of a specific system</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemUsageResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetItSystemUsages(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUsageUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            string systemNameContent = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<ItSystemUsage>>();

            if (organizationUuid.HasValue)
                conditions.Add(new QueryByOrganizationUuid<ItSystemUsage>(organizationUuid.Value));

            if (relatedToSystemUuid.HasValue)
                conditions.Add(new QueryByRelationToSystem(relatedToSystemUuid.Value));

            if (relatedToSystemUsageUuid.HasValue)
                conditions.Add(new QueryByRelationToSystemUsage(relatedToSystemUsageUuid.Value));

            if (systemUuid.HasValue)
                conditions.Add(new QueryBySystemUuid(systemUuid.Value));

            if (!string.IsNullOrWhiteSpace(systemNameContent))
                conditions.Add(new QueryBySystemNameContent(systemNameContent));

            return _itSystemUsageService
                .Query(conditions.ToArray())
                .OrderBy(itSystemUsage => itSystemUsage.Id)
                .Page(paginationQuery).AsEnumerable()
                .Select(_responseMapper.MapSystemUsageDTO).ToList()
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
                .GetByUuid(systemUsageUuid)
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a system usage.
        /// NOTE: this action also clears any incoming relation e.g. relations from other system usages, contracts, projects or data processing registrations.
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
        [Route("")]
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
                .Create(new SystemUsageCreationParameters(request.SystemUuid, request.OrganizationUuid, CreateFullUpdateParameters(request, false))) //Undefined sections are left out from the spec of additional data
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(MapSystemCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Updates the properties of the system usage.
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

            var updateParameters = CreateFullUpdateParameters(request, true); //Enforce data reset in sections which are undefined in the input since this is PUT

            return _writeService
                .Update(systemUsageUuid, updateParameters)
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Updates the general properties of the system usage.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/general")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageGeneralProperties([NonEmptyGuid] Guid systemUsageUuid, [FromBody] GeneralDataUpdateRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Update(systemUsageUuid, new SystemUsageUpdateParameters
                {
                    GeneralProperties = MapFullUpdateGeneralData(request)
                })
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Updates the role assignments of the system usage.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageRoleAssignments([NonEmptyGuid] Guid systemUsageUuid, [FromBody] IEnumerable<RoleAssignmentResponseDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //TODO: Make wrapper methods yo provide a delta object with only the selected section
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the kle deviations of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/kle")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageKleDeviations([NonEmptyGuid] Guid systemUsageUuid, [FromBody] LocalKLEDeviationsRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Update(systemUsageUuid, new SystemUsageUpdateParameters
                {
                    KLE = MapKle(request)
                })
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Updates the external references of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/external-references")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageExternalReferences([NonEmptyGuid] Guid systemUsageUuid, [FromBody][Required] IEnumerable<ExternalReferenceDataDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Update(systemUsageUuid, new SystemUsageUpdateParameters
                {
                    ExternalReferences = MapReferences(request).FromNullable()
                })
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Updates the archiving registrations of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/archiving")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageArchiving([NonEmptyGuid] Guid systemUsageUuid, [FromBody] ArchivingWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //TODO: Make wrapper methods yo provide a delta object with only the selected section
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the GDPR registrations of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/gdpr")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageGdpr([NonEmptyGuid] Guid systemUsageUuid, [FromBody] GDPRWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //TODO: Make wrapper methods yo provide a delta object with only the selected section
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the organizational references for the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/organization-usage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageOrganizationUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] OrganizationUsageWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Update(systemUsageUuid, new SystemUsageUpdateParameters
                {
                    OrganizationalUsage = MapOrganizationalUsage(request)
                })
                .Select(_responseMapper.MapSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates a system relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/system-relations")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(SystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [FromBody] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets a specific relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the system relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid, [FromBody] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deletes a system relation
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

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a complete update object where all values are defined and fallbacks to null are used for sections which are missing
        /// </summary>
        /// <param name="request"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static SystemUsageUpdateParameters CreateFullUpdateParameters(CreateItSystemUsageRequestDTO request, bool enforceUndefinedSections)
        {
            //TODO: Merge this method with the other one - the general section is the issue and the one that diverges based on create vs update
            var generalDataInput = request.General ?? (enforceUndefinedSections ? new GeneralDataWriteRequestDTO() : null);
            var orgUsageInput = request.OrganizationUsage ?? (enforceUndefinedSections ? new OrganizationUsageWriteRequestDTO() : null);
            var kleInput = request.LocalKleDeviations ?? (enforceUndefinedSections ? new LocalKLEDeviationsRequestDTO() : null);
            var externalReferncesInput = request.ExternalReferences ?? (enforceUndefinedSections ? new List<ExternalReferenceDataDTO>() : null);
            return new SystemUsageUpdateParameters
            {
                GeneralProperties = generalDataInput.FromNullable().Select(MapFullCommonGeneralData),
                OrganizationalUsage = orgUsageInput.FromNullable().Select(MapOrganizationalUsage),
                KLE = kleInput.FromNullable().Select(MapKle),
                ExternalReferences = externalReferncesInput.FromNullable().Select(MapReferences)
            };
        }

        /// <summary>
        /// Creates a complete update object where all values are defined and fallbacks to null are used for sections which are missing
        /// </summary>
        /// <param name="request"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static SystemUsageUpdateParameters CreateFullUpdateParameters(UpdateItSystemUsageRequestDTO request, bool enforceUndefinedSections)
        {
            var generalDataInput = request.General ?? (enforceUndefinedSections ? new GeneralDataUpdateRequestDTO() : null);
            var orgUsageInput = request.OrganizationUsage ?? (enforceUndefinedSections ? new OrganizationUsageWriteRequestDTO() : null);
            var kleInput = request.LocalKleDeviations ?? (enforceUndefinedSections ? new LocalKLEDeviationsRequestDTO() : null);
            var externalReferncesInput = request.ExternalReferences ?? (enforceUndefinedSections ? new List<ExternalReferenceDataDTO>() : null);
            return new SystemUsageUpdateParameters
            {
                GeneralProperties = generalDataInput.FromNullable().Select(MapFullUpdateGeneralData),
                OrganizationalUsage = orgUsageInput.FromNullable().Select(MapOrganizationalUsage),
                KLE = kleInput.FromNullable().Select(MapKle),
                ExternalReferences = externalReferncesInput.FromNullable().Select(MapReferences)
            };
        }

        private static IEnumerable<UpdatedExternalReferenceProperties> MapReferences(IEnumerable<ExternalReferenceDataDTO> references)
        {
            return references.Select(x => new UpdatedExternalReferenceProperties
            {
                Title = x.Title,
                DocumentId = x.DocumentId,
                Url = x.Url,
                MasterReference = x.MasterReference
            });
        }

        private static UpdatedSystemUsageKLEDeviationParameters MapKle(LocalKLEDeviationsRequestDTO kle)
        {
            return new UpdatedSystemUsageKLEDeviationParameters()
            {
                AddedKLEUuids = kle.AddedKLEUuids.FromNullable().AsChangedValue(),
                RemovedKLEUuids = kle.RemovedKLEUuids.FromNullable().AsChangedValue()
            };
        }

        /// <summary>
        /// Maps a complete update of the general data update request. Nulled sections are interpreted as intentionally reset
        /// </summary>
        /// <param name="generalData"></param>
        /// <returns></returns>
        private static UpdatedSystemUsageGeneralProperties MapFullUpdateGeneralData(GeneralDataUpdateRequestDTO generalData)
        {
            var generalProperties = MapFullCommonGeneralData(generalData);
            generalProperties.MainContractUuid = (generalData.MainContractUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue();
            return generalProperties;
        }

        private static UpdatedSystemUsageOrganizationalUseParameters MapOrganizationalUsage(OrganizationUsageWriteRequestDTO input)
        {
            return new UpdatedSystemUsageOrganizationalUseParameters
            {
                ResponsibleOrganizationUnitUuid = (input.ResponsibleOrganizationUnitUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                UsingOrganizationUnitUuids = (input.UsingOrganizationUnitUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
            };
        }

        private static UpdatedSystemUsageGeneralProperties MapFullCommonGeneralData(GeneralDataWriteRequestDTO generalData)
        {
            return new UpdatedSystemUsageGeneralProperties
            {
                LocalCallName = generalData.LocalCallName.AsChangedValue(),
                LocalSystemId = generalData.LocalSystemId.AsChangedValue(),
                Notes = generalData.Notes.AsChangedValue(),
                SystemVersion = generalData.SystemVersion.AsChangedValue(),
                DataClassificationUuid = (generalData.DataClassificationUuid?.FromNullable() ?? Maybe<Guid>.None).AsChangedValue(),
                NumberOfExpectedUsersInterval = generalData
                    .NumberOfExpectedUsers?
                    .FromNullable()
                    .Select(interval => (interval.LowerBound.GetValueOrDefault(0), interval.UpperBound)).Match(interval => interval, () => Maybe<(int, int?)>.None)
                    .AsChangedValue(),
                EnforceActive = ((generalData.Validity?.EnforcedValid)?.FromNullable() ?? Maybe<bool>.None).AsChangedValue(),
                ValidFrom = (generalData.Validity?.ValidFrom?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue(),
                ValidTo = (generalData.Validity?.ValidTo?.FromNullable() ?? Maybe<DateTime>.None).AsChangedValue(),
                AssociatedProjectUuids = (generalData.AssociatedProjectUuids?.FromNullable() ?? Maybe<IEnumerable<Guid>>.None).AsChangedValue()
            };
        }

        private CreatedNegotiatedContentResult<ItSystemUsageResponseDTO> MapSystemCreatedResponse(ItSystemUsageResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}