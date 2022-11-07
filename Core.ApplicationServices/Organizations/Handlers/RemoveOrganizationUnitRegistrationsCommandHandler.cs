using Core.Abstractions.Types;
using Core.DomainModel.Commands;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using System;
using System.Linq;

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
            return _organizationUnitService.DeleteRegistrations(command.OrganizationUuid, command.UnitUuid)
                .Match
                (
                    error => error,
                    () =>
                    {
                        RemoveItSystemUsageOrgUnitUsages(command.UnitUuid);
                        return Maybe<OperationError>.None;
                    });
        }

        private void RemoveItSystemUsageOrgUnitUsages(Guid unitUuid)
        {
            var itSystemUsageOrgUnitUsages = _itSystemUsageOrgUnitUsageRepository.AsQueryable().Where(x => x.OrganizationUnit.Uuid == unitUuid);
            foreach (var itSystemUsage in itSystemUsageOrgUnitUsages)
            {
                _itSystemUsageOrgUnitUsageRepository.Delete(itSystemUsage);
            }

            _itSystemUsageOrgUnitUsageRepository.Save();
        }
    }
}
