using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Model.StsOrganization;
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
        [Route("snapshot")] //TODO: Rename to query | preview?
        public HttpResponseMessage GetSnapshotFromStsOrganization(Guid organizationId, uint? levels = null)
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
        public HttpResponseMessage CreateConnection(ConnectToStsOrganizationRequestDTO request)
        {
            //TODO: Perform the import (only allowed to CREATE if not already created). If created, the current one must be updated!
            return Ok();
        }

        //TODO: https://os2web.atlassian.net/browse/KITOSUDV-3313 adds the PUT (POST creates the connection)

        private static StsOrganizationOrgUnitDTO MapOrganizationUnitDTO(StsOrganizationUnit organizationUnit)
        {
            return new StsOrganizationOrgUnitDTO()
            {
                Uuid = organizationUnit.Uuid,
                Name = organizationUnit.Name,
                UserFacingKey = organizationUnit.UserFacingKey,
                Children = organizationUnit.Children.Select(MapOrganizationUnitDTO).ToList()
            };
        }
    }
}