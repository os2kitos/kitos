using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Commands;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.GDPR;


namespace Core.DomainServices.Contract
{
    public class ContractDataProcessingRegistrationAssignmentService : IContractDataProcessingRegistrationAssignmentService
    {
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;
        private readonly ICommandBus _commandBus;

        public ContractDataProcessingRegistrationAssignmentService(IDataProcessingRegistrationRepository dataProcessingRegistrationRepository, 
            ICommandBus commandBus)
        {
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
            _commandBus = commandBus;
        }
        public IQueryable<DataProcessingRegistration> GetApplicableDataProcessingRegistrations(ItContract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            return
                contract
                    .DataProcessingRegistrations
                    .Select(x => x.Id)
                    .ToList()
                    .Transform
                    (
                        idsInUse => _dataProcessingRegistrationRepository
                            .GetDataProcessingRegistrationsFromOrganization(contract.OrganizationId)
                            .ExceptEntitiesWithIds(idsInUse)
                    );
        }

        public Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(ItContract contract, int dataProcessingRegistrationId)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            return _dataProcessingRegistrationRepository
                .GetById(dataProcessingRegistrationId)
                .Match
                (
                    contract.AssignDataProcessingRegistration,
                    () => new OperationError("Data processing registration ID is not valid", OperationFailure.BadInput)
                );
        }

        public Result<DataProcessingRegistration, OperationError> RemoveDataProcessingRegistration(ItContract contract, int dataProcessingRegistrationId)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            return _dataProcessingRegistrationRepository
                .GetById(dataProcessingRegistrationId)
                .Match
                (
                    contract.RemoveDataProcessingRegistration,
                    () => new OperationError("Data processing registration ID is not valid", OperationFailure.BadInput)
                )
                .Bind(removedDpr =>
                {
                    var command = new RemoveMainContractFromDataProcessingRegistrationCommand(removedDpr);
                    return _commandBus.Execute<RemoveMainContractFromDataProcessingRegistrationCommand, Maybe<OperationError>>(command)
                        .Match(error => error, () => Result<DataProcessingRegistration, OperationError>.Success(removedDpr));
                });
        }
    }
}
