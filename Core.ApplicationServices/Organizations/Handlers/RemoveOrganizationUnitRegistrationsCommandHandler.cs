using Core.Abstractions.Types;
using Core.DomainModel.Commands;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class RemoveOrganizationUnitRegistrationsCommandHandler : ICommandHandler<RemoveOrganizationUnitRegistrationsCommand, Maybe<OperationError>>
    {
        private readonly IOrganizationUnitService _organizationUnitService;
        private readonly IGenericRepository<ItSystemUsageOrgUnitUsage> _itSystemUsageOrgUnitUsageRepository;

        public RemoveOrganizationUnitRegistrationsCommandHandler(IOrganizationUnitService organizationUnitService,
            IGenericRepository<ItSystemUsageOrgUnitUsage> itSystemUsageOrgUnitUsageRepository)
        {
            _organizationUnitService = organizationUnitService;
            _itSystemUsageOrgUnitUsageRepository = itSystemUsageOrgUnitUsageRepository;
        }

        public Maybe<OperationError> Execute(RemoveOrganizationUnitRegistrationsCommand command)
        {
            return _organizationUnitService.DeleteAllUnitOrganizationRegistrations(command.OrganizationId, command.UnitId)
                .Match
                (
                    error => error,
                    () =>
                    {
                        RemoveItSystemUsageOrgUnitUsages(command.UnitId);
                        return Maybe<OperationError>.None;
                    });
        }

        private void RemoveItSystemUsageOrgUnitUsages(int unitId)
        {
            var itSystemUsageOrgUnitUsages = _itSystemUsageOrgUnitUsageRepository.Get(x => x.OrganizationUnitId == unitId);
            foreach (var itSystemUsage in itSystemUsageOrgUnitUsages)
            {
                _itSystemUsageOrgUnitUsageRepository.Delete(itSystemUsage);
            }

            _itSystemUsageOrgUnitUsageRepository.Save();
        }
    }
}
