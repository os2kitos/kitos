using System;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationSystemAssignmentService : IDataProcessingRegistrationSystemAssignmentService
    {
        private readonly IItSystemUsageRepository _repository;

        public DataProcessingRegistrationSystemAssignmentService(IItSystemUsageRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<ItSystemUsage> GetApplicableSystems(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return
                registration
                    .SystemUsages
                    .Select(x => x.Id)
                    .ToList()
                    .Transform
                    (
                        idsInUse => _repository
                            .GetSystemUsagesFromOrganization(registration.OrganizationId)
                            .ExceptEntitiesWithIds(idsInUse)
                    );
        }

        public Result<ItSystemUsage, OperationError> AssignSystem(DataProcessingRegistration registration, int systemId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return _repository
                .GetSystemUsage(systemId)
                .FromNullable()
                .Match
                (
                    registration.AssignSystem,
                    () => new OperationError("System ID is not valid", OperationFailure.BadInput)
                );
        }

        public Result<ItSystemUsage, OperationError> RemoveSystem(DataProcessingRegistration registration, int systemId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return _repository
                .GetSystemUsage(systemId)
                .FromNullable()
                .Match
                (
                    registration.RemoveSystem,
                    () => new OperationError("System ID is not valid", OperationFailure.BadInput)
                );
        }
    }
}
