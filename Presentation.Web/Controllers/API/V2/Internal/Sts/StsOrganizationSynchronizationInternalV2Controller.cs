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
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using System.Net;

namespace Presentation.Web.Controllers.API.V2.Internal.Sts
{
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/sts-organization-synchronization")]
    public class StsOrganizationSynchronizationInternalV2Controller : InternalApiV2Controller
    {
        private readonly IStsOrganizationSynchronizationService _stsOrganizationSynchronizationService;

        public StsOrganizationSynchronizationInternalV2Controller(IStsOrganizationSynchronizationService stsOrganizationSynchronizationService)
        {
            _stsOrganizationSynchronizationService = stsOrganizationSynchronizationService;
        }


        [HttpGet]
        [Route("snapshot")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(StsOrganizationOrgUnitDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetSnapshotFromStsOrganization(Guid organizationUuid, int? levels = null)
        {
            return _stsOrganizationSynchronizationService
                .GetStsOrganizationalHierarchy(organizationUuid, levels.FromNullableValueType())
                .Select(MapOrganizationUnitDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("connection-status")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(StsOrganizationSynchronizationDetailsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetSynchronizationStatus(Guid organizationUuid)
        {
            return _stsOrganizationSynchronizationService
                .GetSynchronizationDetails(organizationUuid)
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
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult CreateConnection(Guid organizationUuid, [FromBody] ConnectToStsOrganizationRequestDTO request)
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
                .Connect(organizationUuid, (request?.SynchronizationDepth).FromNullableValueType(), request.SubscribeToUpdates.GetValueOrDefault(false))
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("connection")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult Disconnect(Guid organizationUuid, [FromBody] DisconnectFromStsOrganizationRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("request is null");
            }

            return _stsOrganizationSynchronizationService
                .Disconnect(organizationUuid, request.PurgeUnusedExternalUnits)
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("connection/subscription")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult DeleteSubscription(Guid organizationUuid)
        {
            return _stsOrganizationSynchronizationService
                .UnsubscribeFromAutomaticUpdates(organizationUuid)
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("connection/update")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ConnectionUpdateConsequencesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUpdateConsequences(Guid organizationUuid, int? synchronizationDepth = null)
        {
            if (synchronizationDepth is < 1)
            {
                return BadRequest($"{nameof(synchronizationDepth)} must greater than 0");
            }

            return _stsOrganizationSynchronizationService
                .GetConnectionExternalHierarchyUpdateConsequences(organizationUuid, synchronizationDepth.FromNullableValueType())
                .Select(MapUpdateConsequencesResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpPut]
        [Route("connection")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult UpdateConnection(Guid organizationUuid, [FromBody] ConnectToStsOrganizationRequestDTO request)
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
                .UpdateConnection(organizationUuid, (request?.SynchronizationDepth).FromNullableValueType(), request.SubscribeToUpdates.GetValueOrDefault(false))
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("connection/change-log")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<StsOrganizationChangeLogResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetChangeLogs(Guid organizationUuid, int numberOfChangeLogs)
        {
            return _stsOrganizationSynchronizationService.GetChangeLogs(organizationUuid, numberOfChangeLogs)
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