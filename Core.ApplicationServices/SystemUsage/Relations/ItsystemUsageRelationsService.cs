using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;

using Serilog;

namespace Core.ApplicationServices.SystemUsage.Relations
{
    public class ItsystemUsageRelationsService : IItsystemUsageRelationsService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IOptionsService<SystemRelation, RelationFrequencyType> _frequencyService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<SystemRelation> _relationRepository;
        private readonly IGenericRepository<ItInterface> _interfaceRepository;
        private readonly ILogger _logger;

        public ItsystemUsageRelationsService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemRepository systemRepository,
            IItContractRepository contractRepository,
            IOptionsService<SystemRelation, RelationFrequencyType> frequencyService,
            IGenericRepository<SystemRelation> relationRepository,
            IGenericRepository<ItInterface> interfaceRepository,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            ILogger logger)
        {
            _usageRepository = usageRepository;
            _authorizationContext = authorizationContext;
            _systemRepository = systemRepository;
            _contractRepository = contractRepository;
            _frequencyService = frequencyService;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _relationRepository = relationRepository;
            _interfaceRepository = interfaceRepository;
            _logger = logger;
        }

        public Result<SystemRelation, OperationError> AddRelation(
            int fromSystemUsageId,
            int toSystemUsageId,
            int? interfaceId,
            string description,
            string reference,
            int? frequencyId,
            int? contractId)
        {
            var operationContext = new SystemRelationOperationContext(
                new SystemRelationOperationParameters(fromSystemUsageId, toSystemUsageId, interfaceId, frequencyId, contractId),
                new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(LoadToSystemUsage)
                    .Bind(WithAuthorizedModificationAccess)
                    .Bind(LoadFrequency)
                    .Bind(LoadInterface)
                    .Bind(LoadContract)
                    .Match
                    (
                        onSuccess: context =>
                        {
                            var fromSystemUsage = context.Entities.FromSystemUsage;
                            var toSystemUsage = context.Entities.ToSystemUsage;
                            var frequency = context.Entities.Frequency;
                            var contract = context.Entities.Contract;
                            var relationInterface = context.Entities.Interface;
                            return fromSystemUsage
                                .AddUsageRelationTo(toSystemUsage, relationInterface, description, reference, frequency, contract)
                                .Match<Result<SystemRelation, OperationError>>
                                (
                                    onSuccess: createdRelation =>
                                    {
                                        _usageRepository.Save();
                                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(fromSystemUsage));
                                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(toSystemUsage));
                                        return createdRelation;
                                    },
                                    onFailure: error => error
                                );
                        },
                        onFailure: error => error
                    );
        }

        public Result<SystemRelation, OperationError> ModifyRelation(
            int fromSystemUsageId,
            int relationId,
            int toSystemUsageId,
            string changedDescription,
            string changedReference,
            int? toInterfaceId,
            int? toContractId,
            int? toFrequencyId)
        {
            var operationContext = new SystemRelationOperationContext(
                new SystemRelationOperationParameters(fromSystemUsageId, toSystemUsageId, toInterfaceId, toFrequencyId, toContractId),
                new SystemRelationOperationEntities());

            var originalToSystemUsage = _relationRepository.GetByKey(relationId)?.ToSystemUsage;

            if (originalToSystemUsage == null)
            {
                return Result<SystemRelation, OperationError>.Failure(OperationFailure.NotFound);
            }

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(LoadToSystemUsage)
                    .Bind(WithAuthorizedModificationAccess)
                    .Bind(LoadFrequency)
                    .Bind(LoadInterface)
                    .Bind(LoadContract)
                    .Match
                    (
                        onSuccess: context =>
                        {
                            var fromSystemUsage = context.Entities.FromSystemUsage;
                            var toSystemUsage = context.Entities.ToSystemUsage;
                            var frequency = context.Entities.Frequency;
                            var contract = context.Entities.Contract;
                            var relationInterface = context.Entities.Interface;

                            return fromSystemUsage
                                .ModifyUsageRelation(relationId, toSystemUsage, changedDescription, changedReference, relationInterface, contract, frequency)
                                .Match<Result<SystemRelation, OperationError>>
                                (
                                    onSuccess: modifiedRelation =>
                                    {
                                        if (originalToSystemUsage.Id != toSystemUsageId)
                                        {
                                            _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(originalToSystemUsage));
                                        }
                                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(fromSystemUsage));
                                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(toSystemUsage));
                                        _usageRepository.Save();
                                        return modifiedRelation;
                                    },
                                    onFailure: error => error
                                );
                        },
                        onFailure: error => error
                    );
        }

        public Result<IEnumerable<SystemRelation>, OperationError> GetRelationsAssociatedWithContract(int contractId)
        {
            return _contractRepository
                .GetById(contractId)
                .FromNullable()
                .Select(WithAuthorizedReadAccessToContract)
                .GetValueOrFallback(new OperationError("Contract not found", OperationFailure.NotFound))
                .Match<Result<IEnumerable<SystemRelation>, OperationError>>
                (
                    onSuccess: contract => contract.AssociatedSystemRelations.ToList(),
                    onFailure: error => error
                );
        }

        public Result<IEnumerable<SystemRelation>, OperationError> GetRelationsDefinedInOrganization(int organizationId, int pageNumber, int pageSize)
        {
            if (pageNumber < 0)
                return new OperationError("Page number must be equal to or greater than 0", OperationFailure.BadInput);

            if (pageSize < 1 || pageSize > 100)
                return new OperationError("Page number be within the interval [1,100]", OperationFailure.BadInput);

            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError("Denied access to view local data in organization", OperationFailure.Forbidden);

            return _usageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .SelectMany(systemUsage => systemUsage.UsageRelations)
                .OrderBy(relation => relation.Id)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public Result<IEnumerable<SystemRelation>, OperationError> GetRelationsFrom(int systemUsageId)
        {
            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters { FromSystemUsageId = systemUsageId }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(WithAuthorizedReadAccess)
                    .Match<Result<IEnumerable<SystemRelation>, OperationError>>
                    (
                        onSuccess: context => context
                            .Entities
                            .FromSystemUsage
                            .UsageRelations
                            .ToList(),
                        onFailure: error => error
                    );
        }

        public Result<IEnumerable<SystemRelation>, OperationError> GetRelationsTo(int systemUsageId)
        {
            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters
            {
                FromSystemUsageId = systemUsageId
            }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(WithAuthorizedReadAccess)
                    .Match<Result<IEnumerable<SystemRelation>, OperationError>>
                    (
                        onSuccess: context => context
                            .Entities
                            .FromSystemUsage
                            .UsedByRelations
                            .ToList(),
                        onFailure: error => error
                    );
        }

        public Result<SystemRelation, OperationError> RemoveRelation(int fromSystemUsageId, int relationId)
        {
            using var transaction = _transactionManager.Begin();
            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters { FromSystemUsageId = fromSystemUsageId }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(WithAuthorizedModificationAccess)
                    .Match<Result<SystemRelation, OperationError>>
                    (
                        onSuccess: context => context
                            .Entities
                            .FromSystemUsage
                            .RemoveUsageRelation(relationId)
                            .Match<Result<SystemRelation, OperationError>>
                            (
                                onSuccess: removedRelation =>
                                {
                                    var fromSystemUsage = removedRelation.FromSystemUsage;
                                    var toSystemUsage = removedRelation.ToSystemUsage;
                                    _relationRepository.DeleteWithReferencePreload(removedRelation);
                                    _relationRepository.Save();
                                    _usageRepository.Save();
                                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(fromSystemUsage));
                                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(toSystemUsage));
                                    transaction.Commit();
                                    return removedRelation;
                                },
                                onFailure: error =>
                                {
                                    _logger.Error("Attempt to remove relation from {systemUsageId} with Id {relationId} failed with {error}", fromSystemUsageId, relationId, error);
                                    return new OperationError(error);
                                }),
                        onFailure: error => error
                    );
        }

        public Result<SystemRelation, OperationFailure> GetRelationFrom(int systemUsageId, int relationId)
        {
            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters { FromSystemUsageId = systemUsageId }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(WithAuthorizedReadAccess)
                    .Match
                    (
                        onSuccess: context => context
                            .Entities
                            .FromSystemUsage
                            .GetUsageRelation(relationId)
                            .Select<Result<SystemRelation, OperationFailure>>(relation => relation)
                            .GetValueOrFallback(OperationFailure.NotFound),
                        onFailure: error => error.FailureType
                    );
        }

        public Result<IEnumerable<ItSystemUsage>, OperationError> GetSystemUsagesWhichCanBeRelatedTo(int fromSystemUsageId, Maybe<string> nameContent, int pageSize)
        {
            if (pageSize < 1)
                return new OperationError("Min page size is 1", OperationFailure.BadInput);

            if (pageSize > 25)
                return new OperationError("Max page size is 25", OperationFailure.BadInput);

            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters { FromSystemUsageId = fromSystemUsageId }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(WithAuthorizedReadAccess)
                    .Match<Result<IEnumerable<ItSystemUsage>, OperationError>>
                    (
                        onSuccess: context =>
                        {
                            var fromUsage = context.Entities.FromSystemUsage;
                            var systemsInUse = _systemRepository
                                .GetSystemsInUse(fromUsage.OrganizationId);

                            var idsOfSystemsInUse = nameContent
                                .Select(name => systemsInUse.ByPartOfName(name))
                                .GetValueOrFallback(systemsInUse)
                                .OrderBy(x => x.Name)
                                .Take(pageSize)
                                .Select(x => x.Id)
                                .ToList();

                            return _usageRepository
                                .AsQueryable()
                                .ByOrganizationId(fromUsage.OrganizationId) //Only usages from same organization
                                .ExceptEntitiesWithIds(fromSystemUsageId)   //do not include "from" system
                                .Where(u => idsOfSystemsInUse.Contains(u.ItSystemId))
                                .ToList();
                        },
                        onFailure: error => error
                    );
        }

        public Result<RelationOptionsDTO, OperationError> GetAvailableOptions(int fromSystemUsageId, int toSystemUsageId)
        {
            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters
            {
                FromSystemUsageId = fromSystemUsageId,
                ToSystemUsageId = toSystemUsageId
            }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Bind(LoadToSystemUsage)
                    .Bind(WithAuthorizedReadAccess)
                    .Match<Result<RelationOptionsDTO, OperationError>>
                    (
                        onSuccess: context =>
                        {
                            var fromSystemUsage = context.Entities.FromSystemUsage;
                            var toSystemUsage = context.Entities.ToSystemUsage;

                            if (!fromSystemUsage.IsInSameOrganizationAs(toSystemUsage))
                                return new OperationError("source and destination usages are from different organizations", OperationFailure.BadInput);

                            var availableFrequencyTypes = _frequencyService.GetAvailableOptions(fromSystemUsage.OrganizationId).ToList();
                            var exposedInterfaces = toSystemUsage.GetExposedInterfaces();
                            var contracts = _contractRepository.GetContractsInOrganization(fromSystemUsage.OrganizationId).OrderBy(c => c.Name).ToList();

                            return new RelationOptionsDTO(fromSystemUsage, toSystemUsage, exposedInterfaces, contracts, availableFrequencyTypes);
                        },
                        onFailure: error => error
                    );
        }

        #region Parameter Types

        private class SystemRelationOperationParameters
        {
            public int FromSystemUsageId { get; set; }
            public int ToSystemUsageId { get; set; }
            public int? InterfaceId { get; set; }
            public int? FrequencyId { get; set; }
            public int? ContractId { get; set; }

            public SystemRelationOperationParameters()
            {
            }

            public SystemRelationOperationParameters(int fromSystemUsageId, int systemUsageId, int? interfaceId, int? frequencyId, int? contractId)
            {
                FromSystemUsageId = fromSystemUsageId;
                ToSystemUsageId = systemUsageId;
                InterfaceId = interfaceId;
                FrequencyId = frequencyId;
                ContractId = contractId;
            }
        }

        private class SystemRelationOperationEntities
        {
            public ItSystemUsage FromSystemUsage { get; set; }
            public ItSystemUsage ToSystemUsage { get; set; }
            public Maybe<ItInterface> Interface { get; set; }
            public Maybe<RelationFrequencyType> Frequency { get; set; }
            public Maybe<ItContract> Contract { get; set; }
        }

        private class SystemRelationOperationContext
        {
            public SystemRelationOperationParameters Input { get; }
            public SystemRelationOperationEntities Entities { get; }

            public SystemRelationOperationContext(SystemRelationOperationParameters input, SystemRelationOperationEntities entities)
            {
                Input = input;
                Entities = entities;
            }
        }

        #endregion Parameter Types
        #region helpers

        private Result<ItContract, OperationError> WithAuthorizedReadAccessToContract(ItContract contract)
        {
            return _authorizationContext.AllowReads(contract) ?
                Result<ItContract, OperationError>.Success(contract) :
                new OperationError("Not allowed to read contract", OperationFailure.Forbidden);
        }

        private Result<SystemRelationOperationContext, OperationError> LoadFromSystemUsage(SystemRelationOperationContext context)
        {
            var fromSystemUsage = _usageRepository.GetByKey(context.Input.FromSystemUsageId);
            if (fromSystemUsage == null)
                return new OperationError("'From' not found", OperationFailure.NotFound);

            context.Entities.FromSystemUsage = fromSystemUsage;
            return context;
        }

        private Result<SystemRelationOperationContext, OperationError> WithAuthorizedModificationAccess(SystemRelationOperationContext context)
        {
            return !_authorizationContext.AllowModify(context.Entities.FromSystemUsage)
                ? Result<SystemRelationOperationContext, OperationError>.Failure(OperationFailure.Forbidden)
                : context;
        }

        private Result<SystemRelationOperationContext, OperationError> WithAuthorizedReadAccess(SystemRelationOperationContext context)
        {
            return !_authorizationContext.AllowReads(context.Entities.FromSystemUsage)
                ? Result<SystemRelationOperationContext, OperationError>.Failure(OperationFailure.Forbidden)
                : context;
        }

        private Result<SystemRelationOperationContext, OperationError> LoadToSystemUsage(SystemRelationOperationContext context)
        {
            var toSystemUsage = _usageRepository.GetByKey(context.Input.ToSystemUsageId);

            if (toSystemUsage == null)
                return new OperationError("'To' could not be found", OperationFailure.BadInput);

            context.Entities.ToSystemUsage = toSystemUsage;
            return context;
        }

        private Result<SystemRelationOperationContext, OperationError> LoadFrequency(SystemRelationOperationContext context)
        {
            var toFrequency = Maybe<RelationFrequencyType>.None;
            var frequencyId = context.Input.FrequencyId;
            if (frequencyId.HasValue)
            {
                toFrequency = _frequencyService.GetAvailableOption(context.Entities.FromSystemUsage.OrganizationId, frequencyId.Value);

                if (toFrequency.IsNone)
                {
                    return new OperationError("Frequency type is not available in the organization", OperationFailure.BadInput);
                }
            }

            context.Entities.Frequency = toFrequency.GetValueOrDefault();
            return context;
        }

        private Result<SystemRelationOperationContext, OperationError> LoadInterface(SystemRelationOperationContext context)
        {
            var toInterface = Maybe<ItInterface>.None;
            var interfaceId = context.Input.InterfaceId;
            if (interfaceId.HasValue)
            {
                toInterface = _interfaceRepository.GetByKey(interfaceId.Value);
                if (toInterface.IsNone)
                {
                    return new OperationError("The provided interface id does not point to a valid interface", OperationFailure.BadInput);
                }
            }

            context.Entities.Interface = toInterface.GetValueOrDefault();

            return context;
        }

        private Result<SystemRelationOperationContext, OperationError> LoadContract(SystemRelationOperationContext context)
        {
            var toContract = Maybe<ItContract>.None;
            var contractId = context.Input.ContractId;
            if (contractId.HasValue)
            {
                toContract = _contractRepository.GetById(contractId.Value);
                if (toContract.IsNone)
                {
                    return new OperationError("Contract id does not point to a valid contract", OperationFailure.BadInput);
                }
            }

            context.Entities.Contract = toContract.GetValueOrDefault();

            return context;
        }
        #endregion helpers
    }
}
