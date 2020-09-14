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
    public class DataProcessingAgreementSystemAssignmentService : IDataProcessingAgreementSystemAssignmentService
    {
        private readonly IItSystemRepository _repository;

        public DataProcessingAgreementSystemAssignmentService(IItSystemRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<ItSystem> GetApplicableSystems(DataProcessingAgreement agreement)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));

            return
                agreement
                    .SystemUsages
                    .Select(x => x.ItSystem.Id)
                    .ToList()
                    .Transform
                    (
                        idsInUse => _repository
                            .GetSystemsInUse(agreement.OrganizationId)
                            .ExceptEntitiesWithIds(idsInUse)
                    );
        }

        public Result<ItSystem, OperationError> AssignSystem(DataProcessingAgreement agreement, int systemId)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));

            return _repository
                .GetSystem(systemId)
                .FromNullable()
                .Match
                (
                    agreement.AssignSystem,
                    () => new OperationError("System ID is not valid", OperationFailure.BadInput)
                );
        }

        public Result<ItSystem, OperationError> RemoveSystem(DataProcessingAgreement agreement, int systemId)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));
            return _repository
                .GetSystem(systemId)
                .FromNullable()
                .Match
                (
                    agreement.RemoveSystem,
                    () => new OperationError("System ID is not valid", OperationFailure.BadInput)
                );
        }
    }
}
