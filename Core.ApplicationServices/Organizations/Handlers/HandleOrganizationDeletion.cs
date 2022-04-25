using System;
using System.Linq;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices.Context;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class HandleOrganizationDeletion : IDomainEventHandler<EntityBeingDeletedEvent<Organization>>
    {
        private readonly IItContractWriteService _contractService;
        private readonly IItSystemUsageWriteService _itSystemUsageService;
        private readonly IItSystemService _itSystemService;
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;
        private readonly IItProjectService _projectService;
        private readonly IItInterfaceService _interfaceService;
        private readonly IOrganizationService _organizationService;
        private readonly IDefaultOrganizationResolver _defaultOrganizationResolver;

        public HandleOrganizationDeletion(
            IItContractWriteService contractService,
            IItSystemUsageWriteService itSystemUsageService,
            IItSystemService itSystemService,
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationService,
            IItProjectService projectService,
            IItInterfaceService interfaceService,
            IOrganizationService organizationService,
            IDefaultOrganizationResolver defaultOrganizationResolver)
        {
            _contractService = contractService;
            _itSystemUsageService = itSystemUsageService;
            _itSystemService = itSystemService;
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
            _projectService = projectService;
            _interfaceService = interfaceService;
            _organizationService = organizationService;
            _defaultOrganizationResolver = defaultOrganizationResolver;
        }

        public void Handle(EntityBeingDeletedEvent<Organization> domainEvent)
        {
            var organization = domainEvent.Entity;
            var defaultOrganization = _defaultOrganizationResolver.Resolve();
            var conflicts = _organizationService.ComputeOrganizationRemovalConflicts(organization.Uuid).Value;

            //Clearing organization on contracts where it is set as supplier
            var organizationSupplier = conflicts.ContractsInOtherOrganizationsWhereOrgIsSupplier.ToList();
            organizationSupplier.ForEach(x => _contractService.Update(x.Uuid, new ItContractModificationParameters { Supplier = new ItContractSupplierModificationParameters() { OrganizationUuid = OptionalValueChange<Guid?>.With(null) } })); //TODO: Check result
            organization.Supplier.Clear();

            //Removing registrations on DPRs where organization is set as data processor or sub data processor
            var subDataProcessorContext = conflicts.DprInOtherOrganizationsWhereOrgIsSubDataProcessor.ToList();
            subDataProcessorContext.ForEach(x => _dataProcessingRegistrationService.RemoveSubDataProcessor(x.Id, organization.Id));//TODO: Check result
            organization.SubDataProcessorForDataProcessingRegistrations.Clear();

            var dataProcessorContext = conflicts.DprInOtherOrganizationsWhereOrgIsDataProcessor.ToList();
            dataProcessorContext.ForEach(x => _dataProcessingRegistrationService.RemoveDataProcessor(x.Id, organization.Id));//TODO: Check result
            organization.DataProcessorForDataProcessingRegistrations.Clear();

            //Removing registration on it-systems where organization is set as rightsholder
            var itSystems = conflicts.SystemsInOtherOrganizationsWhereOrgIsRightsHolder.ToList();
            itSystems.ForEach(x => _itSystemService.UpdateRightsHolder(x.Id, null)); //TODO: Check result
            organization.BelongingSystems.Clear();

            //Removing contracts created in the org
            var itContracts = organization.ItContracts.ToList();
            itContracts.ForEach(c => _contractService.Delete(c.Uuid)); //TODO: Check result
            organization.ItContracts.Clear();

            //Removing system usages created in the organization
            var itSystemUsages = organization.ItSystemUsages.ToList();
            itSystemUsages.ForEach(x => _itSystemUsageService.Delete(x.Uuid)); //TODO: Check result
            organization.ItSystemUsages.Clear();

            //Removing DPRs created in the organization
            var dprs = organization.DataProcessingRegistrations.ToList();
            dprs.ForEach(x => _dataProcessingRegistrationService.Delete(x.Id)); //TODO: Check result
            organization.DataProcessingRegistrations.Clear();

            //Removing Projects created in the organization
            var itProjects = organization.ItProjects.ToList();
            itProjects.ForEach(x => _projectService.DeleteProject(x.Id)); //TODO: Check result
            organization.ItProjects.Clear();

            //Move systems which are used on global objects outside the organization into the "Default org"
            var systemsToBeMovedToDefaultOrganization = conflicts
                .SystemsWithUsagesOutsideTheOrganization
                .Concat(conflicts.SystemsExposingInterfacesDefinedInOtherOrganizations)
                .Concat(conflicts.SystemsSetAsParentSystemToSystemsInOtherOrganizations)
                .ToList();

            systemsToBeMovedToDefaultOrganization.ForEach(system => system.ChangeOrganization(defaultOrganization));

            //Move interfaces which are used on global objects outside the organization into the "Default org"
            var interfacesToBeMovedToDefaultOrganization =
                conflicts
                    .InterfacesExposedOnSystemsOutsideTheOrganization
                    .ToList();
            interfacesToBeMovedToDefaultOrganization.ForEach(itInterface => itInterface.ChangeOrganization(defaultOrganization));

            var movedSystemIds = systemsToBeMovedToDefaultOrganization.Select(x => x.Id).Distinct().ToHashSet();
            var movedInterfaceIds = interfacesToBeMovedToDefaultOrganization.Select(x => x.Id).Distinct().ToHashSet();

            //Removing it-interfaces created in the organization
            var itInterfaces = organization.ItInterfaces.Where(x => movedInterfaceIds.Contains(x.Id) == false).ToList();
            itInterfaces.ForEach(x => _interfaceService.UpdateExposingSystem(x.Id, null));//TODO: Check result
            itInterfaces.ForEach(x => _interfaceService.Delete(x.Id));//TODO: Check result
            organization.ItInterfaces.Clear();

            //Removing IT-Systems created in the organization
            var systems = organization.ItSystems.Where(x => movedSystemIds.Contains(x.Id) == false).ToList();
            systems.ForEach(x => x.Children.ToList().ForEach(x.RemoveChildSystem));
            systems.ForEach(x => _itSystemService.Delete(x.Id)); //TODO: Check result
            organization.ItSystems.Clear();

            //TODO: Add a boolean (break dependencies to the delete methods.. then deep domain knowledge does not bleed into this class)
        }
    }
}
