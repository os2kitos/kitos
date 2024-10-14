using Core.Abstractions.Extensions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Sts.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Core.ApplicationServices.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.API.V2.Internal.Sts
{
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/sts-organization-synchronization")]
    public class StsOrganiationSynchronizationInternalV2Controller : InternalApiV2Controller
    {
        private readonly IStsOrganizationSynchronizationService _stsOrganizationSynchronizationService;

        public StsOrganiationSynchronizationInternalV2Controller(IStsOrganizationSynchronizationService stsOrganizationSynchronizationService)
        {
            _stsOrganizationSynchronizationService = stsOrganizationSynchronizationService;
        }


        [HttpGet]
        [Route("snapshot")]
        public IHttpActionResult GetSnapshotFromStsOrganization(Guid organizationId, int? levels = null)
        {
            return _stsOrganizationSynchronizationService
                .GetStsOrganizationalHierarchy(organizationId, levels.FromNullableValueType())
                .Select(MapOrganizationUnitDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("connection-status")]
        public IHttpActionResult GetSynchronizationStatus(Guid organizationId)
        {
            return _stsOrganizationSynchronizationService
                .GetSynchronizationDetails(organizationId)
                .Select(details => new StsOrganizationSynchronizationDetailsResponseDTO
                {
                    Connected = details.Connected,
                    SubscribesToUpdates = details.SubscribesToUpdates,
                    DateOfLatestCheckBySubscription = details.DateOfLatestCheckBySubscription,
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
        public IHttpActionResult CreateConnection(Guid organizationId, [FromBody] ConnectToStsOrganizationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request == null)
            {
                return BadRequest("Invalid request body");
            }

            return _stsOrganizationSynchronizationService
                .Connect(organizationId, (request?.SynchronizationDepth).FromNullableValueType(), request.SubscribeToUpdates.GetValueOrDefault(false))
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("connection")]
        public IHttpActionResult Disconnect(Guid organizationId, [FromBody] DisconnectFromStsOrganizationRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("request is null");
            }

            return _stsOrganizationSynchronizationService
                .Disconnect(organizationId, request.PurgeUnusedExternalUnits)
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("connection/subscription")]
        public IHttpActionResult DeleteSubscription(Guid organizationId)
        {
            return _stsOrganizationSynchronizationService
                .UnsubscribeFromAutomaticUpdates(organizationId)
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("connection/update")]
        public IHttpActionResult GetUpdateConsequences(Guid organizationId, int? synchronizationDepth = null)
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
        public IHttpActionResult UpdateConnection(Guid organizationId, [FromBody] ConnectToStsOrganizationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request == null)
            {
                return BadRequest("Invalid request body");
            }

            return _stsOrganizationSynchronizationService
                .UpdateConnection(organizationId, (request?.SynchronizationDepth).FromNullableValueType(), request.SubscribeToUpdates.GetValueOrDefault(false))
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("connection/change-log")]
        public IHttpActionResult GetChangeLogs(Guid organizationId, int numberOfChangeLogs)
        {
            return _stsOrganizationSynchronizationService.GetChangeLogs(organizationId, numberOfChangeLogs)
                .Select(MapChangeLogResponseDtos)
                .Match(Ok, FromOperationError);
        }

        #region DTO Mapping
        private ConnectionUpdateConsequencesResponseDTO MapUpdateConsequencesResponseDTO(OrganizationTreeUpdateConsequences consequences)
        {
            var logEntries = consequences
                .ConvertConsequencesToConsequenceLogs()
                .Transform(x => MapConsequenceLogsToDtos(x))
                .Transform(OrderLogEntries);

            return new ConnectionUpdateConsequencesResponseDTO
            {
                Consequences = logEntries
            };
        }

        private static List<ConnectionUpdateOrganizationUnitConsequenceDTO> OrderLogEntries(IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> logEntries)
        {
            var consequenceDtos = logEntries
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Category)
                .ToList();
            return consequenceDtos;
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
        private static IEnumerable<StsOrganizationChangeLogResponseDTO> MapChangeLogResponseDtos(IEnumerable<IExternalConnectionChangelog> logs)
        {
            return logs.Select(MapChangeLogResponseDto).ToList();
        }

        private static StsOrganizationChangeLogResponseDTO MapChangeLogResponseDto(IExternalConnectionChangelog log)
        {
            return new StsOrganizationChangeLogResponseDTO
            {
                Origin = log.ResponsibleType.ToStsOrganizationChangeLogOriginOption(),
                User = log.ResponsibleUser.FromNullable().Select(x => x.MapUserReferenceResponseDTO()).GetValueOrDefault(),
                Consequences = MapConsequenceLogsToDtos(log.GetEntries()),
                LogTime = log.LogTime
            };
        }

        private static IEnumerable<ConnectionUpdateOrganizationUnitConsequenceDTO> MapConsequenceLogsToDtos(IEnumerable<IExternalConnectionChangeLogEntry> logs)
        {
            return logs
                .Select(MapConsequenceToDto)
                .Transform(OrderLogEntries)
                .ToList();
        }

        private static ConnectionUpdateOrganizationUnitConsequenceDTO MapConsequenceToDto(IExternalConnectionChangeLogEntry log)
        {
            return new ConnectionUpdateOrganizationUnitConsequenceDTO
            {
                Uuid = log.ExternalUnitUuid,
                Name = log.Name,
                Category = log.Type,
                Description = log.Description
            };
        }
        #endregion DTO Mapping
    }
}