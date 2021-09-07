using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Contract;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Contract;
using Infrastructure.Services.DataAccess;

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
        private readonly IOrganizationService _organizationService;

        public ItContractService(
            IItContractRepository repository,
            IGenericRepository<EconomyStream> economyStreamRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IAuthorizationContext authorizationContext,
            ILogger logger,
            IContractDataProcessingRegistrationAssignmentService contractDataProcessingRegistrationAssignmentService,
            IOrganizationService organizationService)
        {
            _repository = repository;
            _economyStreamRepository = economyStreamRepository;
            _referenceService = referenceService;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _authorizationContext = authorizationContext;
            _logger = logger;
            _contractDataProcessingRegistrationAssignmentService = contractDataProcessingRegistrationAssignmentService;
            _organizationService = organizationService;
        }

        public Result<ItContract, OperationError> Create(int organizationId, string name)
        {
            using var transaction = _transactionManager.Begin();

            if (!_authorizationContext.AllowCreate<ItContract>(organizationId))
                return new OperationError(OperationFailure.Forbidden);
            var result = CanCreateNewContractWithName(name, organizationId);

            if (result.Failed)
                return result.Error;

            if (!result.Value)
                return new OperationError("Name taken", OperationFailure.Conflict);

            var itContract = new ItContract { OrganizationId = organizationId, Name = name };
            itContract = _repository.Add(itContract);
            _domainEvents.Raise(new EntityCreatedEvent<ItContract>(itContract));
            transaction.Commit();

            return itContract;
        }

        public IQueryable<ItContract> GetAllByOrganization(int orgId, string optionalNameSearch = null)
        {
            var contracts = _repository.GetContractsInOrganization(orgId);

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
            using (var transaction = _transactionManager.Begin())
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

        public Result<IQueryable<ItContract>, OperationError> GetContractsInOrganization(Guid organizationUuid, params IDomainQuery<ItContract>[] conditions)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind(organization =>
                {
                    var query = new IntersectionQuery<ItContract>(conditions);

                    return _repository
                        .GetContractsInOrganization(organization.Id)
                        .Transform(query.Apply)
                        .Transform(Result<IQueryable<ItContract>, OperationError>.Success);
                });
        }

        public Result<ItContract, OperationError> GetContract(Guid uuid)
        {
            return _repository
                .GetContract(uuid)
                .Match
                (
                    contract => _authorizationContext.AllowReads(contract) ? Result<ItContract, OperationError>.Success(contract) : new OperationError(OperationFailure.Forbidden),
                    () => new OperationError(OperationFailure.NotFound)
                );
        }

        public Result<bool, OperationError> CanCreateNewContractWithName(string name, int organizationId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return SearchByName(organizationId, name).Any() == false;
        }

        public Maybe<OperationError> ValidateNewName(int contractId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new OperationError(OperationFailure.BadInput);

            return
                _repository
                    .GetById(contractId)
                    .FromNullable()
                    .Match(WithReadAccess, () => new OperationError(OperationFailure.NotFound))
                    .Select(project => SearchByName(project.OrganizationId, name).ExceptEntitiesWithIds(contractId).Any())
                    .Match
                    (
                        overlapsFound =>
                            overlapsFound ? new OperationError(OperationFailure.Conflict) : Maybe<OperationError>.None,
                        error => error
                    );
        }

        private IQueryable<ItContract> SearchByName(int organizationId, string name)
        {
            return _repository.GetContractsInOrganization(organizationId).ByNameExact(name);
        }

        private Result<ItContract, OperationError> WithReadAccess(ItContract contract)
        {
            return _authorizationContext.AllowReads(contract) ? Result<ItContract, OperationError>.Success(contract) : new OperationError(OperationFailure.Forbidden);
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
            using var transaction = _transactionManager.Begin();

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
