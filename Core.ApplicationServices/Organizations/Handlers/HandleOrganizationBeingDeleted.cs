using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Exceptions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.Model.SystemUsage.Write;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Context;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class HandleOrganizationBeingDeleted : IDomainEventHandler<EntityBeingDeletedEvent<Organization>>
    {
        private readonly IItContractWriteService _contractService;
        private readonly IItSystemUsageWriteService _itSystemUsageService;
        private readonly IItSystemService _itSystemService;
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;
        private readonly IItInterfaceService _interfaceService;
        private readonly IOrganizationService _organizationService;
        private readonly IDefaultOrganizationResolver _defaultOrganizationResolver;
        private readonly IDomainEvents _domainEvents;

        public HandleOrganizationBeingDeleted(
            IItContractWriteService contractService,
            IItSystemUsageWriteService itSystemUsageService,
            IItSystemService itSystemService,
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationService,
            IItInterfaceService interfaceService,
            IOrganizationService organizationService,
            IDefaultOrganizationResolver defaultOrganizationResolver,
            IDomainEvents domainEvents)
        {
            _contractService = contractService;
            _itSystemUsageService = itSystemUsageService;
            _itSystemService = itSystemService;
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
            _interfaceService = interfaceService;
            _organizationService = organizationService;
            _defaultOrganizationResolver = defaultOrganizationResolver;
            _domainEvents = domainEvents;
        }

        public void Handle(EntityBeingDeletedEvent<Organization> domainEvent)
        {
            var organization = domainEvent.Entity;
            var defaultOrganization = _defaultOrganizationResolver.Resolve();
            var conflicts = _organizationService.ComputeOrganizationRemovalConflicts(organization.Uuid).Value;

            ResolveContractConflicts(conflicts, organization);

            ResolveDprConflicts(conflicts, organization);

            ResolveRightsHolderConflicts(conflicts, organization);

            ResolveArchiveSupplierConflicts(conflicts, organization);

            ClearLocalRegistrations(organization);

            var systemsToBeMovedToDefaultOrganization = ResolveGlobalItSystemConflicts(conflicts, defaultOrganization);

            var interfacesToBeMovedToDefaultOrganization = ResolveGlobalItInterfacesConflicts(conflicts, defaultOrganization);

            ClearInterfaces(interfacesToBeMovedToDefaultOrganization, organization);

            ClearItSystems(systemsToBeMovedToDefaultOrganization, organization);
        }

        private void ClearItSystems(IEnumerable<ItSystem> systemsMovedToDefaultOrganization, Organization organization)
        {
            //Removing IT-Systems created in the organization and which have not been moved to the default
            var movedSystemIds = systemsMovedToDefaultOrganization.Select(x => x.Id).Distinct().ToHashSet();

            var systems = organization.ItSystems.Where(x => movedSystemIds.Contains(x.Id) == false).ToList();
            systems.ForEach(x =>
            {
                var systemDeleteResult = _itSystemService.Delete(x.Id, true);
                if (systemDeleteResult != SystemDeleteResult.Ok)
                {
                    throw new OperationErrorException<SystemDeleteResult>(systemDeleteResult);
                }
            });
            organization.ItSystems.Clear();
        }

        private void ClearInterfaces(IEnumerable<ItInterface> interfacesMovedToDefaultOrganization, Organization organization)
        {
            //Removing it-interfaces created in the organization and which are not already moved
            var movedInterfaceIds = interfacesMovedToDefaultOrganization.Select(x => x.Id).Distinct().ToHashSet();
            var itInterfaces = organization.ItInterfaces.Where(x => movedInterfaceIds.Contains(x.Id) == false).ToList();
            itInterfaces.ForEach(x => _interfaceService.Delete(x.Id, true).ThrowOnFailure());
            organization.ItInterfaces.Clear();
        }

        private IEnumerable<ItInterface> ResolveGlobalItInterfacesConflicts(OrganizationRemovalConflicts conflicts, Organization defaultOrganization)
        {
            //Move interfaces which are used on global objects outside the organization into the "Default org"
            var interfacesToBeMovedToDefaultOrganization =
                conflicts
                    .InterfacesExposedOnSystemsOutsideTheOrganization
                    .ToList();
            interfacesToBeMovedToDefaultOrganization.ForEach(itInterface =>
            {
                itInterface.ChangeOrganization(defaultOrganization);
                _domainEvents.Raise(new EntityUpdatedEvent<ItInterface>(itInterface));
            });
            return interfacesToBeMovedToDefaultOrganization;
        }

        private IEnumerable<ItSystem> ResolveGlobalItSystemConflicts(OrganizationRemovalConflicts conflicts, Organization defaultOrganization)
        {
            //Move systems which are used on global objects outside the organization into the "Default org"
            var systemsToBeMovedToDefaultOrganization = conflicts
                .SystemsWithUsagesOutsideTheOrganization
                .Concat(conflicts.SystemsExposingInterfacesDefinedInOtherOrganizations)
                .Concat(conflicts.SystemsSetAsParentSystemToSystemsInOtherOrganizations)
                .ToList();

            systemsToBeMovedToDefaultOrganization.ForEach(system =>
            {
                system.ChangeOrganization(defaultOrganization);
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(system));
            });
            return systemsToBeMovedToDefaultOrganization;
        }

        private void ClearLocalRegistrations(Organization organization)
        {
            //Removing contracts created in the org
            var itContracts = organization.ItContracts.ToList();
            itContracts.ForEach(c => _contractService.Delete(c.Uuid).ThrowOnValue());
            organization.ItContracts.Clear();

            //Removing system usages created in the organization
            var itSystemUsages = organization.ItSystemUsages.ToList();
            itSystemUsages.ForEach(x => _itSystemUsageService.Delete(x.Uuid).ThrowOnValue());
            organization.ItSystemUsages.Clear();

            //Removing DPRs created in the organization
            var dprs = organization.DataProcessingRegistrations.ToList();
            dprs.ForEach(x => _dataProcessingRegistrationService.Delete(x.Id).ThrowOnFailure());
            organization.DataProcessingRegistrations.Clear();
        }

        private void ResolveRightsHolderConflicts(OrganizationRemovalConflicts conflicts, Organization organization)
        {
            //Removing registration on it-systems where organization is set as rightsholder
            var itSystems = conflicts.SystemsInOtherOrganizationsWhereOrgIsRightsHolder.ToList();
            itSystems.ForEach(x => _itSystemService.UpdateRightsHolder(x.Id, null).ThrowOnFailure());
            organization.BelongingSystems.Clear();
        }

        private void ResolveDprConflicts(OrganizationRemovalConflicts conflicts, Organization organization)
        {
            //Removing registrations on DPRs where organization is set as data processor or sub data processor
            var subDataProcessorContext = conflicts.DprInOtherOrganizationsWhereOrgIsSubDataProcessor.ToList();
            subDataProcessorContext.ForEach(x =>
                _dataProcessingRegistrationService.RemoveSubDataProcessor(x.Id, organization.Id).ThrowOnFailure());
            organization.SubDataProcessorRegistrations.Clear();

            var dataProcessorContext = conflicts.DprInOtherOrganizationsWhereOrgIsDataProcessor.ToList();
            dataProcessorContext.ForEach(x =>
                _dataProcessingRegistrationService.RemoveDataProcessor(x.Id, organization.Id).ThrowOnFailure());
            organization.DataProcessorForDataProcessingRegistrations.Clear();
        }

        private void ResolveContractConflicts(OrganizationRemovalConflicts conflicts, Organization organization)
        {
            //Clearing organization on contracts where it is set as supplier
            var organizationSupplier = conflicts.ContractsInOtherOrganizationsWhereOrgIsSupplier.ToList();
            organizationSupplier.ForEach(x => _contractService.Update(x.Uuid,
                new ItContractModificationParameters
                {
                    Supplier = new ItContractSupplierModificationParameters()
                    { OrganizationUuid = OptionalValueChange<Guid?>.With(null) }
                }).ThrowOnFailure()); 
            organization.Supplier.Clear();
        }

        private void ResolveArchiveSupplierConflicts(OrganizationRemovalConflicts conflicts, Organization organization)
        {
            //Clearing organization on systems where it is set as supplier
            var organizationSupplier = conflicts.SystemUsagesWhereOrgIsArchiveSupplier.ToList();
            organizationSupplier.ForEach(x => _itSystemUsageService.Update(x.Uuid,
                new SystemUsageUpdateParameters
                {
                    Archiving = new UpdatedSystemUsageArchivingParameters
                    {
                        ArchiveSupplierOrganizationUuid = Maybe<Guid>.None.AsChangedValue()
                    }
                }).ThrowOnFailure()); 
        }
    }
}
