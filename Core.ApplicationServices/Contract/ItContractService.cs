using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Contracts;
using Core.ApplicationServices.References;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Contract;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
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
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOptionsService<ItContract, CriticalityType> _criticalityOptionsService;
        private readonly IOptionsService<ItContract, ItContractType> _contractTypeOptionsService;
        private readonly IOptionsService<ItContract, ItContractTemplateType> _contractTemplateOptionsService;
        private readonly IOptionsService<ItContract, PurchaseFormType> _purchaseFormOptionsService;
        private readonly IOptionsService<ItContract, ProcurementStrategyType> _procurementStrategyOptionsService;
        private readonly IOptionsService<ItContract, PaymentModelType> _paymentModelOptionsService;
        private readonly IOptionsService<ItContract, PaymentFreqencyType> _paymentFrequencyOptionsService;
        private readonly IOptionsService<ItContract, OptionExtendType> _optionExtendOptionsService;
        private readonly IOptionsService<ItContract, TerminationDeadlineType> _terminationDeadlineOptionsService;

        public ItContractService(
            IItContractRepository repository,
            IGenericRepository<EconomyStream> economyStreamRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IAuthorizationContext authorizationContext,
            ILogger logger,
            IContractDataProcessingRegistrationAssignmentService contractDataProcessingRegistrationAssignmentService, 
            IOrganizationalUserContext userContext,
            IOptionsService<ItContract, CriticalityType> criticalityOptionsService, 
            IOptionsService<ItContract, ItContractType> contractTypeOptionsService,
            IOptionsService<ItContract, ItContractTemplateType> contractTemplateOptionsService, 
            IOptionsService<ItContract, PurchaseFormType> purchaseFormOptionsService, 
            IOptionsService<ItContract, ProcurementStrategyType> procurementStrategyOptionsService,
            IOptionsService<ItContract, PaymentModelType> paymentModelOptionsService,
            IOptionsService<ItContract, PaymentFreqencyType> paymentFrequencyOptionsService,
            IOptionsService<ItContract, OptionExtendType> optionExtendOptionsService, 
            IOptionsService<ItContract, TerminationDeadlineType> terminationDeadlineOptionsService)
        {
            _repository = repository;
            _economyStreamRepository = economyStreamRepository;
            _referenceService = referenceService;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _authorizationContext = authorizationContext;
            _logger = logger;
            _contractDataProcessingRegistrationAssignmentService = contractDataProcessingRegistrationAssignmentService;
            _userContext = userContext;
            _criticalityOptionsService = criticalityOptionsService;
            _contractTypeOptionsService = contractTypeOptionsService;
            _contractTemplateOptionsService = contractTemplateOptionsService;
            _purchaseFormOptionsService = purchaseFormOptionsService;
            _procurementStrategyOptionsService = procurementStrategyOptionsService;
            _paymentModelOptionsService = paymentModelOptionsService;
            _paymentFrequencyOptionsService = paymentFrequencyOptionsService;
            _optionExtendOptionsService = optionExtendOptionsService;
            _terminationDeadlineOptionsService = terminationDeadlineOptionsService;
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
                    _domainEvents.Raise(new EntityBeingDeletedEvent<ItContract>(contract));
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
                    .ByPartOfName(nameQuery)
                    .OrderBy(x => x.Id)
                    .Take(pageSize)
                    .OrderBy(x => x.Name)
                    .ToList()
            );
        }

        public IQueryable<ItContract> Query(params IDomainQuery<ItContract>[] conditions)
        {
            var baseQuery = _repository.AsQueryable();
            var subQueries = new List<IDomainQuery<ItContract>>();

            if (_authorizationContext.GetCrossOrganizationReadAccess() < CrossOrganizationDataReadAccessLevel.All)
                subQueries.Add(new QueryByOrganizationIds<ItContract>(_userContext.OrganizationIds));

            subQueries.AddRange(conditions);

            var result = subQueries.Any()
                ? new IntersectionQuery<ItContract>(subQueries).Apply(baseQuery)
                : baseQuery;

            return result;
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

        public Result<ContractOptions, OperationError> GetAssignableContractOptions(int organizationId)
        {
            return WithOrganizationReadAccess(organizationId,
                () => new ContractOptions(
                    _criticalityOptionsService.GetAllOptionsDetails(organizationId),
                    _contractTypeOptionsService.GetAllOptionsDetails(organizationId),
                    _contractTemplateOptionsService.GetAllOptionsDetails(organizationId),
                    _purchaseFormOptionsService.GetAllOptionsDetails(organizationId),
                    _procurementStrategyOptionsService.GetAllOptionsDetails(organizationId),
                    _paymentModelOptionsService.GetAllOptionsDetails(organizationId),
                    _paymentFrequencyOptionsService.GetAllOptionsDetails(organizationId),
                    _optionExtendOptionsService.GetAllOptionsDetails(organizationId),
                    _terminationDeadlineOptionsService.GetAllOptionsDetails(organizationId)));
        }

        public IEnumerable<ItContract> GetAvailableProcurementPlans(int organizationId)
        {
            var contracts = GetAllByOrganization(organizationId);

            return contracts.Where(contract => contract.ProcurementPlanYear != null && contract.ProcurementPlanQuarter != null).ToList();
        }

        private Result<ContractOptions, OperationError> WithOrganizationReadAccess(int organizationId, Func<Result<ContractOptions, OperationError>> authorizedAction)
        {
            var readAccessLevel = _authorizationContext.GetOrganizationReadAccessLevel(organizationId);

            return readAccessLevel < OrganizationDataReadAccessLevel.All
                ? new OperationError(OperationFailure.Forbidden)
                : authorizedAction();
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
