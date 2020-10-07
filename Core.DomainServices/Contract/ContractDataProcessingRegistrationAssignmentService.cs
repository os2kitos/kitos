using System;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.GDPR;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Contract
{
    public class ContractDataProcessingRegistrationAssignmentService : IContractDataProcessingRegistrationAssignmentService
    {
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;

        public ContractDataProcessingRegistrationAssignmentService(IDataProcessingRegistrationRepository dataProcessingRegistrationRepository)
        {
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
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
                .GetProcessingRegistration(dataProcessingRegistrationId)
                .FromNullable()
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
                .GetProcessingRegistration(dataProcessingRegistrationId)
                .FromNullable()
                .Match
                (
                    contract.RemoveDataProcessingRegistration,
                    () => new OperationError("Data processing registration ID is not valid", OperationFailure.BadInput)
                );
        }
    }
}
