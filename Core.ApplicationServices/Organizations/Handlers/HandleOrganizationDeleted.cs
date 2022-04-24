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

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class HandleOrganizationDeleted : IDomainEventHandler<EntityDeletedEvent<Organization>>
    {
        private readonly IItContractWriteService _contractService;
        private readonly IItSystemUsageWriteService _itSystemUsageService;
        private readonly IItSystemService _itSystemService;
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;
        private readonly IItProjectService _projectService;
        private readonly IItInterfaceService _interfaceService;

        public HandleOrganizationDeleted(
            IItContractWriteService contractService,
            IItSystemUsageWriteService itSystemUsageService,
            IItSystemService itSystemService,
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationService,
            IItProjectService projectService,
            IItInterfaceService interfaceService)
        {
            _contractService = contractService;
            _itSystemUsageService = itSystemUsageService;
            _itSystemService = itSystemService;
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
            _projectService = projectService;
            _interfaceService = interfaceService;
        }

        public void Handle(EntityDeletedEvent<Organization> domainEvent)
        {
            var organization = domainEvent.Entity;

            //Clearing organization on contracts where it is set as supplier
            var organizationSupplier = organization.Supplier.ToList();
            organizationSupplier.ForEach(x => _contractService.Update(x.Uuid, new ItContractModificationParameters { Supplier = new ItContractSupplierModificationParameters() { OrganizationUuid = OptionalValueChange<Guid?>.With(null) } }));
            organization.Supplier.Clear();//TODO: Check result

            //Removing registrations on DPRs where organization is set as data processor or sub data processor
            var subDataProcessorContext = organization.SubDataProcessorForDataProcessingRegistrations.ToList();
            subDataProcessorContext.ForEach(x => _dataProcessingRegistrationService.RemoveSubDataProcessor(x.Id, organization.Id));//TODO: Check result
            organization.SubDataProcessorForDataProcessingRegistrations.Clear();

            var dataProcessorContext = organization.DataProcessorForDataProcessingRegistrations.ToList();
            dataProcessorContext.ForEach(x => _dataProcessingRegistrationService.RemoveDataProcessor(x.Id, organization.Id));//TODO: Check result
            organization.DataProcessorForDataProcessingRegistrations.Clear();

            //Removing registration on it-systems where organization is set as rightsholder
            var itSystems = organization.BelongingSystems.ToList();//TODO: Check result
            itSystems.ForEach(x => _itSystemService.UpdateRightsHolder(x.Id, null));
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

            //Removing IT-Systems created in the organization
            var systems = organization.ItSystems.ToList();
            systems.ForEach(x=>_itSystemService.Delete(x.Id)); //TODO: Check result
            organization.ItSystems.Clear(); 
            
            //Removing it-interfaces created in the organization
            var itInterfaces = organization.ItInterfaces.ToList();
            itInterfaces.ForEach(x=>_interfaceService.Delete(x.Id));//TODO: Check result
            organization.ItInterfaces.Clear();
        }
    }
}
