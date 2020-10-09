using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.References;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Contract;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.ApplicationServices.Contract
{
    public class ItContractService : IItContractService
    {
        private readonly IGenericRepository<EconomyStream> _economyStreamRepository;
        private readonly IReferenceService _referenceService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItContractRepository _repository;
        private readonly ILogger _logger;
        private readonly IContractDataProcessingRegistrationAssignmentService _contractDataProcessingRegistrationAssignmentService;

        public ItContractService(
            IItContractRepository repository,
            IGenericRepository<EconomyStream> economyStreamRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IAuthorizationContext authorizationContext,
            ILogger logger,
            IContractDataProcessingRegistrationAssignmentService contractDataProcessingRegistrationAssignmentService)
        {
            _repository = repository;
            _economyStreamRepository = economyStreamRepository;
            _referenceService = referenceService;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _authorizationContext = authorizationContext; 
            _logger = logger;
            _contractDataProcessingRegistrationAssignmentService = contractDataProcessingRegistrationAssignmentService;
        }

        public IQueryable<ItContract> GetAllByOrganization(int orgId, string optionalNameSearch = null)
        {
            var contracts = _repository.GetByOrganizationId(orgId);

            if (!string.IsNullOrWhiteSpace(optionalNameSearch))
            {
                contracts = contracts.ByPartOfName(optionalNameSearch);
            }

            return contracts;
        }

        public Result<ItContract, OperationFailure> Delete(int id)
        {
            var contract = _repository.GetById(id);

            if (contract == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowDelete(contract))
            {
                return OperationFailure.Forbidden;
            }
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                try
                {
                    //Delete the economy streams to prevent them from being orphaned
                    foreach (var economyStream in GetEconomyStreams(contract))
                    {
                        DeleteEconomyStream(economyStream);
                    }
                    _economyStreamRepository.Save();

                    //Delete the contract
                    var deleteByContractId = _referenceService.DeleteByContractId(id);
                    if (deleteByContractId.Failed)
                    {
                        transaction.Rollback();
                        return deleteByContractId.Error;
                    }
                    _domainEvents.Raise(new ContractDeleted(contract));
                    _repository.DeleteContract(contract);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Failed to delete it contract with id: {contract.Id}");
                    transaction.Rollback();
                    return OperationFailure.UnknownError;
                }
            }
            return contract;
            
        }

        public Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(int id, int dataProcessingRegistrationId)
        {
            return Modify(id, contract => _contractDataProcessingRegistrationAssignmentService.AssignDataProcessingRegistration(contract, dataProcessingRegistrationId));
        }

        public Result<DataProcessingRegistration, OperationError> RemoveDataProcessingRegistration(int id, int dataProcessingRegistrationId)
        {
            return Modify(id, contract => _contractDataProcessingRegistrationAssignmentService.RemoveDataProcessingRegistration(contract, dataProcessingRegistrationId));
        }

        public Result<IEnumerable<DataProcessingRegistration>, OperationError> GetDataProcessingRegistrationsWhichCanBeAssigned(int id, string nameQuery, int pageSize)
        {
            if (string.IsNullOrEmpty(nameQuery)) throw new ArgumentException($"{nameof(nameQuery)} must be defined");
            if (pageSize < 1) throw new ArgumentException($"{nameof(pageSize)} must be above 0");

            return WithReadAccess<IEnumerable<DataProcessingRegistration>>(id, contract =>
                _contractDataProcessingRegistrationAssignmentService
                    .GetApplicableDataProcessingRegistrations(contract)
                    .Where(x => x.Name.Contains(nameQuery))
                    .OrderBy(x => x.Id)
                    .Take(pageSize)
                    .OrderBy(x => x.Name)
                    .ToList()
            );
        }

        private static IEnumerable<EconomyStream> GetEconomyStreams(ItContract contract)
        {
            return contract
                .ExternEconomyStreams
                .ToList()
                .Concat(contract.InternEconomyStreams.ToList());
        }

        private void DeleteEconomyStream(EconomyStream economyStream)
        {
            _economyStreamRepository.DeleteWithReferencePreload(economyStream);
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(int id, Func<ItContract, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var contract = _repository.GetById(id);

            if (contract == null)
                return new OperationError(OperationFailure.NotFound);

            if (!_authorizationContext.AllowModify(contract))
                return new OperationError(OperationFailure.Forbidden);

            var mutationResult = mutation(contract);

            if (mutationResult.Ok)
            {
                _repository.Update(contract);
                transaction.Commit();
            }

            return mutationResult;
        }

        private Result<TSuccess, OperationError> WithReadAccess<TSuccess>(int id, Func<ItContract, Result<TSuccess, OperationError>> authorizedAction)
        {
            var contract = _repository.GetById(id);

            if (contract == null)
                return new OperationError(OperationFailure.NotFound);

            if (!_authorizationContext.AllowReads(contract))
                return new OperationError(OperationFailure.Forbidden);

            return authorizedAction(contract);
        }
    }
}
