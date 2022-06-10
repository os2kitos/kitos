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
        [Route("snapshot")]
        public HttpResponseMessage GetSnapshotFromStsOrganization(Guid organizationId, uint? levels = null)
        {
            return _stsOrganizationSynchronizationService
                .GetStsOrganizationalHierarchy(organizationId, levels.FromNullableValueType())
                .Select(MapOrganizationUnitDTO)
                .Match(Ok, FromOperationError);
        }

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