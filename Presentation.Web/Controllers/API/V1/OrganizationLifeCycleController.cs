using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Controllers.API.V1
{
    /// <summary>
    /// Internal API to control life cycle of organizations
    /// </summary>
    [InternalApi]
    [RoutePrefix("api/v1/organizations")]
    public class OrganizationLifeCycleController : BaseApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationLifeCycleController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [Route("{organizationUuid}/deletion/conflicts")]
        public HttpResponseMessage GetDeletionConflicts(Guid organizationUuid)
        {
            return
            _organizationService
                .ComputeOrganizationRemovalConflicts(organizationUuid)
                .Select(ToConflictsDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{organizationUuid}/deletion")]
        public HttpResponseMessage DeleteOrganization(Guid organizationUuid, bool enforce = false)
        {
            return _organizationService
                .RemoveOrganization(organizationUuid, enforce)
                .Select(FromOperationError)
                .GetValueOrFallback(Ok());
        }

        private static OrganizationRemovalConflictsDTO ToConflictsDTO(OrganizationRemovalConflicts result)
        {
            return new OrganizationRemovalConflictsDTO
            (
                MapSystemsWithUsagesOutsideTheOrganization(result),
                MapInterfacesExposedOnSystemsOutsideTheOrganization(result),
                MapSystemsExposingInterfacesDefinedInOtherOrganizations(result),
                MapSystemsSetAsParentSystemToSystemsInOtherOrganizations(result),
                result.DprInOtherOrganizationsWhereOrgIsDataProcessor.Select(MapToEntityWithOrganizationRelationshipDto).ToList(),
                result.DprInOtherOrganizationsWhereOrgIsSubDataProcessor.Select(MapToEntityWithOrganizationRelationshipDto).ToList(),
                result.ContractsInOtherOrganizationsWhereOrgIsSupplier.Select(MapToEntityWithOrganizationRelationshipDto).OrderBy(x=> x.Organization.Name).ToList(),
                result.SystemsInOtherOrganizationsWhereOrgIsRightsHolder.Select(MapToEntityWithOrganizationRelationshipDto).ToList(),
                MapSystemsWhereOrgIsArchiveSupplier(result)
            );
        }

        private static IEnumerable<SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO> MapSystemsSetAsParentSystemToSystemsInOtherOrganizations(OrganizationRemovalConflicts result)
        {
            return result
                .SystemsSetAsParentSystemToSystemsInOtherOrganizations
                .Select
                    (
                        system => new SystemSetAsParentSystemToSystemsInOtherOrganizationsConflictDTO
                            (
                                system.MapToNamedEntityDTO(),
                                system
                                    .Children
                                    .Where(child => child.OrganizationId != system.OrganizationId)
                                    .Select(MapToEntityWithOrganizationRelationshipDto)
                                    .ToList()
                                )
                    )
                .ToList();
        }

        private static EntityWithOrganizationRelationshipDTO MapToEntityWithOrganizationRelationshipDto<T>(T entity) where T : IHasId, IHasName, IOwnedByOrganization
        {
            return entity.MapToEntityWithOrganizationRelationshipDTO();
        }

        private static IEnumerable<SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO> MapSystemsExposingInterfacesDefinedInOtherOrganizations(OrganizationRemovalConflicts result)
        {
            return result
                .SystemsExposingInterfacesDefinedInOtherOrganizations
                .Select
                (
                    system => new SystemExposingInterfacesDefinedInOtherOrganizationsConflictDTO
                    (
                        system.MapToNamedEntityDTO(),
                        system
                            .ItInterfaceExhibits
                            .Select(exhibit => exhibit.ItInterface)
                            .Where(itInterface => itInterface.OrganizationId != system.OrganizationId)
                            .Select(itInterface => itInterface.MapToEntityWithOrganizationRelationshipDTO())
                            .ToList()
                        )
                )
                .ToList();
        }

        private static IEnumerable<InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO> MapInterfacesExposedOnSystemsOutsideTheOrganization(OrganizationRemovalConflicts result)
        {
            return result
                .InterfacesExposedOnSystemsOutsideTheOrganization
                .Select
                (
                    itInterface => new InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO
                    (
                        itInterface.MapToNamedEntityDTO(),
                        itInterface.ExhibitedBy.ItSystem.MapToEntityWithOrganizationRelationshipDTO()
                    )
                )
                .ToList();
        }

        private static IEnumerable<SystemWithUsageOutsideOrganizationConflictDTO> MapSystemsWithUsagesOutsideTheOrganization(OrganizationRemovalConflicts result)
        {
            return result
                .SystemsWithUsagesOutsideTheOrganization
                .Select
                    (
                        system => new SystemWithUsageOutsideOrganizationConflictDTO
                            (
                                system.MapToNamedEntityDTO(),
                                system
                                    .Usages
                                    .Select(usage => usage.Organization)
                                    .Where(org => org.Id != system.OrganizationId)
                                    .MapToShallowOrganizationDTOs()
                                    .OrderBy(dto => dto.Name)
                                    .ToList()
                                )
                        )
                .ToList();
        }

        private static IEnumerable<EntityWithOrganizationRelationshipDTO> MapSystemsWhereOrgIsArchiveSupplier(OrganizationRemovalConflicts result)
        {
            return result
                .SystemUsagesWhereOrgIsArchiveSupplier
                .Select(itSystemUsage => new EntityWithOrganizationRelationshipDTO(itSystemUsage.Id, itSystemUsage.ItSystem.Name, itSystemUsage.Organization.MapToShallowOrganizationDTO()))
                .ToList();
        }
    }
}