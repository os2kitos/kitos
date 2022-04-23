using System.Linq;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class HandleOrganizationDeleted : IDomainEventHandler<EntityDeletedEvent<Organization>>
    {
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _itSystemUsageService;
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;
        private readonly IItProjectService _projectService;

        public HandleOrganizationDeleted(
            IItContractService contractService,
            IItSystemUsageService itSystemUsageService,
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationService,
            IItProjectService projectService)
        {
            _contractService = contractService;
            _itSystemUsageService = itSystemUsageService;
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
            _projectService = projectService;
        }

        public void Handle(EntityDeletedEvent<Organization> domainEvent)
        {
            var organization = domainEvent.Entity;

            var itContracts = organization.ItContracts.ToList();
            itContracts.ForEach(c => _contractService.Delete(c.Id)); //TODO: Check result
            organization.ItContracts.Clear();


            var itSystemUsages = organization.ItSystemUsages.ToList();
            itSystemUsages.ForEach(x => _itSystemUsageService.Delete(x.Id)); //TODO: Check result
            organization.ItSystemUsages.Clear();

            var dprs = organization.DataProcessingRegistrations.ToList();
            dprs.ForEach(x => _dataProcessingRegistrationService.Delete(x.Id)); //TODO: Check result
            organization.DataProcessingRegistrations.Clear();

            var itProjects = organization.ItProjects.ToList();
            itProjects.ForEach(x => _projectService.DeleteProject(x.Id)); //TODO: Check result
            organization.ItProjects.Clear();

            //organization.ItSystems.Clear(); //TODO: What about the case where local creations are deleted? .. move ownership to "fælles"?
            //organization.ItInterfaces.Clear();//TODO: What about the case where local creations are deleted? .. move ownership to "fælles"?
        }
}
}
