using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/organizations/{organizationId}/sts-organization-synchronization")]
    public class StsOrganizationSynchronizationController : BaseApiController
    {
        private readonly IStsOrganizationSynchronizationService _stsOrganizationSynchronizationService;

        public StsOrganizationSynchronizationController(IStsOrganizationSynchronizationService stsOrganizationSynchronizationService)
        {
            _stsOrganizationSynchronizationService = stsOrganizationSynchronizationService;
        }

        [HttpGet]
        [Route("snapshot")]
        public HttpResponseMessage GetSnapshotFromStsOrganization(Guid organizationId, int? levels = null)
        {
            return _stsOrganizationSynchronizationService
                .GetStsOrganizationalHierarchy(organizationId, levels.FromNullableValueType())
                .Select(MapOrganizationUnitDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("connection-status")]
        public HttpResponseMessage GetSynchronizationStatus(Guid organizationId)
        {
            return _stsOrganizationSynchronizationService
                .GetSynchronizationDetails(organizationId)
                .Select(details => new StsOrganizationSynchronizationDetailsResponseDTO
                {
                    Connected = details.Connected,
                    SynchronizationDepth = details.SynchronizationDepth,
                    CanCreateConnection = details.CanCreateConnection,
                    CanDeleteConnection = details.CanDeleteConnection,
                    CanUpdateConnection = details.CanUpdateConnection,
                    AccessStatus = new StsOrganizationAccessStatusResponseDTO
                    {
                        AccessGranted = details.CheckConnectionError == null,
                        Error = details.CheckConnectionError
                    }
                })
                .Match(Ok, FromOperationError);

        }

        [HttpPost]
        [Route("connection")]
        public HttpResponseMessage CreateConnection(Guid organizationId, [FromBody] ConnectToStsOrganizationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return _stsOrganizationSynchronizationService
                .Connect(organizationId, (request?.SynchronizationDepth).FromNullableValueType())
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("connection")]
        public HttpResponseMessage Disconnect(Guid organizationId)
        {
            return _stsOrganizationSynchronizationService
                .Disconnect(organizationId)
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("connection/update")]
        public HttpResponseMessage GetUpdateConsequences(Guid organizationId, int? synchronizationDepth = null)
        {
            if (synchronizationDepth is < 1)
            {
                return BadRequest($"{nameof(synchronizationDepth)} must greater than 0");
            }

            return _stsOrganizationSynchronizationService
                .GetConnectionExternalHierarchyUpdateConsequences(organizationId, synchronizationDepth.FromNullableValueType())
                .Select(MapUpdateConsequencesResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpPut]
        [Route("connection")]
        public HttpResponseMessage UpdateConnection(Guid organizationId, [FromBody] ConnectToStsOrganizationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return _stsOrganizationSynchronizationService
                .UpdateConnection(organizationId, (request?.SynchronizationDepth).FromNullableValueType().Value)
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("changelog/{numberOfLastChangeLogs}")]
        public HttpResponseMessage GetLastNumberOfChangeLogsForOrganization(Guid organizationId, int numberOfLastChangeLogs)
        {
            return _stsOrganizationSynchronizationService.GetChangeLogForOrganization(organizationId, numberOfLastChangeLogs)
                .Select(MapChangeLogResponseDtoIEnumerable)
                .Match(Ok, FromOperationError);
        }

        #region DTO Mapping
        private static ConnectionUpdateConsequencesResponseDTO MapUpdateConsequencesResponseDTO(OrganizationTreeUpdateConsequences consequences)
        {
            var dtos = new List<ConnectionUpdateOrganizationUnitConsequenceDTO>();
            dtos.AddRange(MapAddedOrganizationUnits(consequences));
            dtos.AddRange(MapRenamedOrganizationUnits(consequences));
            dtos.AddRange(MapMovedOrganizationUnits(consequences));
            dtos.AddRange(MapRemovedOrganizationUnits(consequences));
            dtos.AddRange(MapConvertedOrganizationUnits(consequences));
            return new ConnectionUpdateConsequencesResponseDTO
            {
                Consequences = dtos
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.Category)
                    .ToList()
            };
        }

        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapConvertedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingConvertedToNativeUnits
                .Select(converted => new ConnectionUpdateOrganizationUnitConsequenceDTO
                {
                    Name = converted.organizationUnit.Name,
                    Category = ConnectionUpdateOrganizationUnitChangeCategory.Converted,
                    Uuid = converted.externalOriginUuid.GetValueOrDefault(),
                    Description = $"'{converted.organizationUnit.Name}' er slettet i FK Organisation men konverteres til KITOS enhed, da den anvendes aktivt i KITOS."
                })
                .ToList();
        }

        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapRemovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .DeletedExternalUnitsBeingDeleted
                .Select(deleted => new ConnectionUpdateOrganizationUnitConsequenceDTO
                {
                    Name = deleted.organizationUnit.Name,
                    Category = ConnectionUpdateOrganizationUnitChangeCategory.Deleted,
                    Uuid = deleted.externalOriginUuid.GetValueOrDefault(),
                    Description = $"'{deleted.organizationUnit.Name}' slettes."
                })
                .ToList();
        }

        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapMovedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingMoved
                .Select(moved =>
                {
                    var (movedUnit, oldParent, newParent) = moved;
                    return new ConnectionUpdateOrganizationUnitConsequenceDTO
                    {
                        Name = movedUnit.Name,
                        Category = ConnectionUpdateOrganizationUnitChangeCategory.Moved,
                        Uuid = movedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{movedUnit.Name}' flyttes fra at være underenhed til '{oldParent.Name}' til fremover at være underenhed for {newParent.Name}"
                    };
                })
                .ToList();
        }

        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapRenamedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .OrganizationUnitsBeingRenamed
                .Select(renamed =>
                {
                    var (affectedUnit, oldName, newName) = renamed;
                    return new ConnectionUpdateOrganizationUnitConsequenceDTO
                    {
                        Name = oldName,
                        Category = ConnectionUpdateOrganizationUnitChangeCategory.Renamed,
                        Uuid = affectedUnit.ExternalOriginUuid.GetValueOrDefault(),
                        Description = $"'{oldName}' omdøbes til '{newName}'"
                    };
                })
                .ToList();
        }

        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapAddedOrganizationUnits(OrganizationTreeUpdateConsequences consequences)
        {
            return consequences
                .AddedExternalOrganizationUnits
                .Select(added => new ConnectionUpdateOrganizationUnitConsequenceDTO
                {
                    Name = added.unitToAdd.Name,
                    Category = ConnectionUpdateOrganizationUnitChangeCategory.Added,
                    Uuid = added.unitToAdd.Uuid,
                    Description = $"'{added.unitToAdd.Name}' tilføjes som underenhed til '{added.parent.Name}'"
                }
                )
                .ToList();
        }

        private static StsOrganizationOrgUnitDTO MapOrganizationUnitDTO(ExternalOrganizationUnit organizationUnit)
        {
            return new StsOrganizationOrgUnitDTO
            {
                Uuid = organizationUnit.Uuid,
                Name = organizationUnit.Name,
                Children = organizationUnit
                    .Children
                    .OrderBy(x => x.Name)
                    .Select(MapOrganizationUnitDTO)
                    .ToList()
            };
        }
        private static IEnumerable<StsOrganizationChangeLogResponseDTO> MapChangeLogResponseDtoIEnumerable(IEnumerable<StsOrganizationChangeLog> logs)
        {
            return logs.Select(MapChangeLogResponseDto).ToList();
        }

        private static StsOrganizationChangeLogResponseDTO MapChangeLogResponseDto(StsOrganizationChangeLog log)
        {
            return new StsOrganizationChangeLogResponseDTO
            {
                Origin = log.Origin.ToStsOrganizationChangeLogOriginOption(),
                Name = log.Name,
                Consequences = MapConsequencesToDtos(log.ConsequenceLogs),
                LogTime = log.LogTime
            };
        }
        
        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapConsequencesToDtos(
            IEnumerable<StsOrganizationConsequenceLog> logs)
        {
            return logs.ToList().Select(MapConsequenceToDto).ToList();
        }

        private static ConnectionUpdateOrganizationUnitConsequenceDTO MapConsequenceToDto(StsOrganizationConsequenceLog log)
        {
            return new ConnectionUpdateOrganizationUnitConsequenceDTO
            {
                Uuid = log.Uuid,
                Name = log.Name,
                Category = log.Type.ToConnectionUpdateOrganizationUnitChangeCategory(),
                Description = log.Description
            };
        }
        #endregion DTO Mapping
    }
}