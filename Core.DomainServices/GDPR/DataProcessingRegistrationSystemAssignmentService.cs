using System;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationSystemAssignmentService : IDataProcessingRegistrationSystemAssignmentService
    {
        private readonly IItSystemRepository _repository;

        public DataProcessingRegistrationSystemAssignmentService(IItSystemRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<ItSystem> GetApplicableSystems(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return
                registration
                    .SystemUsages
                    .Select(x => x.ItSystem.Id)
                    .ToList()
                    .Transform
                    (
                        idsInUse => _repository
                            .GetSystemsInUse(registration.OrganizationId)
                            .ExceptEntitiesWithIds(idsInUse)
                    );
        }

        public Result<ItSystem, OperationError> AssignSystem(DataProcessingRegistration registration, int systemId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return _repository
                .GetSystem(systemId)
                .FromNullable()
                .Match
                (
                    registration.AssignSystem,
                    () => new OperationError("System ID is not valid", OperationFailure.BadInput)
                );
        }

        public Result<ItSystem, OperationError> RemoveSystem(DataProcessingRegistration registration, int systemId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return _repository
                .GetSystem(systemId)
                .FromNullable()
                .Match
                (
                    registration.RemoveSystem,
                    () => new OperationError("System ID is not valid", OperationFailure.BadInput)
                );
        }
    }
}
