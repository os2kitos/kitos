using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
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

            //TODO: Define consequences DTO - Consider exposing it using odata???????
            throw new NotImplementedException("yet");
        }

        [HttpPut]
        [Route("connection")]
        public HttpResponseMessage UpdateConnection(Guid organizationId, [FromBody] ConnectToStsOrganizationRequestDTO request)
        {
            throw new NotImplementedException("yet");
        }

        private static StsOrganizationOrgUnitDTO MapOrganizationUnitDTO(ExternalOrganizationUnit organizationUnit)
        {
            return new StsOrganizationOrgUnitDTO
            {
                Uuid = organizationUnit.Uuid,
                Name = organizationUnit.Name,
                Children = organizationUnit.Children.Select(MapOrganizationUnitDTO).ToList()
            };
        }
    }
}