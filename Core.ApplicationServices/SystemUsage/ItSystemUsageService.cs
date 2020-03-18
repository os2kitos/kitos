using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.DomainEvents;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Serilog;

namespace Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IOptionsService<SystemRelation, RelationFrequencyType> _frequencyService;
        private readonly IOrganizationalUserContext _userContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<SystemRelation> _relationRepository;
        private readonly IGenericRepository<ItInterface> _interfaceRepository;
        private readonly IGenericRepository<ItSystemUsageSensitiveDataLevel> _sensitiveDataLevelRepository;
        private readonly IReferenceService _referenceService;
        private readonly ILogger _logger;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemRepository systemRepository,
            IItContractRepository contractRepository,
            IOptionsService<SystemRelation, RelationFrequencyType> frequencyService,
            IOrganizationalUserContext userContext,
            IGenericRepository<SystemRelation> relationRepository,
            IGenericRepository<ItInterface> interfaceRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            ILogger logger, 
            IGenericRepository<ItSystemUsageSensitiveDataLevel> sensitiveDataLevelRepository)
        {
            _usageRepository = usageRepository;
            _authorizationContext = authorizationContext;
            _systemRepository = systemRepository;
            _contractRepository = contractRepository;
            _frequencyService = frequencyService;
            _userContext = userContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _relationRepository = relationRepository;
            _interfaceRepository = interfaceRepository;
            _referenceService = referenceService;
            _logger = logger;
            _sensitiveDataLevelRepository = sensitiveDataLevelRepository;
        }

        public Result<ItSystemUsage, OperationFailure> Add(ItSystemUsage newSystemUsage, User objectOwner)
        {
            // create the system usage
            var existing = GetByOrganizationAndSystemId(newSystemUsage.OrganizationId, newSystemUsage.ItSystemId);
            if (existing != null)
            {
                return OperationFailure.Conflict;
            }

            if (!_authorizationContext.AllowCreate<ItSystemUsage>(newSystemUsage))
            {
                return OperationFailure.Forbidden;
            }

            var itSystem = _systemRepository.GetSystem(newSystemUsage.ItSystemId);
            if (itSystem == null)
            {
                return OperationFailure.BadInput;
            }

            if (!_authorizationContext.AllowReads(itSystem))
            {
                return OperationFailure.Forbidden;
            }

            //Cannot create system usage in an org where the logical it system is unavailable to the users.
            if (!AllowUsageInTargetOrganization(newSystemUsage, itSystem))
            {
                return OperationFailure.Forbidden;
            }

            var usage = _usageRepository.Create();

            usage.ItSystemId = newSystemUsage.ItSystemId;
            usage.OrganizationId = newSystemUsage.OrganizationId;
            usage.ObjectOwner = objectOwner;
            usage.LastChangedByUser = objectOwner;
            _usageRepository.Insert(usage);
            _usageRepository.Save(); // abuse this as UoW

            return usage;
        }

        private static bool AllowUsageInTargetOrganization(ItSystemUsage newSystemUsage, ItSystem itSystem)
        {
            return
                    newSystemUsage.OrganizationId == itSystem.OrganizationId || //It system is defined in same org as usage
                    itSystem.AccessModifier == AccessModifier.Public;           //It system is public and it is OK to place usages outside the owning organization
        }

        public Result<ItSystemUsage, OperationFailure> Delete(int id)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var itSystemUsage = GetById(id);
                if (itSystemUsage == null)
                {
                    return OperationFailure.NotFound;
                }
                if (!_authorizationContext.AllowDelete(itSystemUsage))
                {
                    return OperationFailure.Forbidden;
                }

                // delete it system usage
                var deleteBySystemUsageId = _referenceService.DeleteBySystemUsageId(id);
                if (deleteBySystemUsageId.Failed)
                {
                    transaction.Rollback();
                    return deleteBySystemUsageId.Error;
                }
                _domainEvents.Raise(new SystemUsageDeleted(itSystemUsage));
                _usageRepository.DeleteByKeyWithReferencePreload(id);
                _usageRepository.Save();
                transaction.Commit();
                return itSystemUsage;
            }
        }

        public ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId)
        {
            return _usageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .FirstOrDefault(u => u.ItSystemId == systemId);
        }

        public ItSystemUsage GetById(int usageId)
        {
            return _usageRepository.GetByKey(usageId);
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
                    .Select(LoadToSystemUsage)
                    .Select(WithAuthorizedModificationAccess)
                    .Select(LoadFrequency)
                    .Select(LoadInterface)
                    .Select(LoadContract)
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
                                .AddUsageRelationTo(_userContext.UserEntity, toSystemUsage, relationInterface, description, reference, frequency, contract)
                                .Match<Result<SystemRelation, OperationError>>
                                (
                                    onSuccess: createdRelation =>
                                    {
                                        _usageRepository.Save();
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

            return
                LoadFromSystemUsage(operationContext)
                    .Select(LoadToSystemUsage)
                    .Select(WithAuthorizedModificationAccess)
                    .Select(LoadFrequency)
                    .Select(LoadInterface)
                    .Select(LoadContract)
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
                                .ModifyUsageRelation(_userContext.UserEntity, relationId, toSystemUsage, changedDescription, changedReference, relationInterface, contract, frequency)
                                .Match<Result<SystemRelation, OperationError>>
                                (
                                    onSuccess: createdRelation =>
                                    {
                                        _usageRepository.Save();
                                        return createdRelation;
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
                    .Select(WithAuthorizedReadAccess)
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
                    .Select(WithAuthorizedReadAccess)
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

        public Result<SystemRelation, OperationFailure> RemoveRelation(int fromSystemUsageId, int relationId)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters { FromSystemUsageId = fromSystemUsageId }, new SystemRelationOperationEntities());

                return
                    LoadFromSystemUsage(operationContext)
                        .Select(WithAuthorizedModificationAccess)
                        .Match
                        (
                            onSuccess: context => context
                                .Entities
                                .FromSystemUsage
                                .RemoveUsageRelation(relationId)
                                .Match<Result<SystemRelation, OperationFailure>>
                                (
                                    onSuccess: removedRelation =>
                                    {
                                        _relationRepository.DeleteWithReferencePreload(removedRelation);
                                        _relationRepository.Save();
                                        _usageRepository.Save();
                                        transaction.Commit();
                                        return removedRelation;
                                    },
                                    onFailure: error =>
                                    {
                                        _logger.Error("Attempt to remove relation from {systemUsageId} with Id {relationId} failed with {error}", fromSystemUsageId, relationId, error);
                                        return error;
                                    }),
                            onFailure: error => error.FailureType
                        );
            }
        }

        public Result<SystemRelation, OperationFailure> GetRelationFrom(int systemUsageId, int relationId)
        {
            var operationContext = new SystemRelationOperationContext(new SystemRelationOperationParameters { FromSystemUsageId = systemUsageId }, new SystemRelationOperationEntities());

            return
                LoadFromSystemUsage(operationContext)
                    .Select(WithAuthorizedReadAccess)
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
                    .Select(WithAuthorizedReadAccess)
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
                    .Select(LoadToSystemUsage)
                    .Select(WithAuthorizedReadAccess)
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
                            var contracts = _contractRepository.GetByOrganizationId(fromSystemUsage.OrganizationId).OrderBy(c => c.Name).ToList();

                            return new RelationOptionsDTO(fromSystemUsage, toSystemUsage, exposedInterfaces, contracts, availableFrequencyTypes);
                        },
                        onFailure: error => error
                    );
        }

        public Result<ItSystemUsageSensitiveDataLevel, OperationError> AddSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            Maybe<ItSystemUsage> usageResult = _usageRepository.GetByKey(itSystemUsageId);

            if (usageResult.IsNone)
                return new OperationError(OperationFailure.BadInput);

            var usage = usageResult.Value;
            if (!_authorizationContext.AllowModify(usage))
                return new OperationError(OperationFailure.Forbidden);

            return usage
                .AddSensitiveDataLevel(_userContext.UserEntity, sensitiveDataLevel)
                .Match<Result<ItSystemUsageSensitiveDataLevel, OperationError>>
                (
                    onSuccess: addedSensitivityLevel =>
                    {
                        _usageRepository.Save();
                        return addedSensitivityLevel;
                    },
                    onFailure: error => error
                );
        }

        public Result<ItSystemUsageSensitiveDataLevel, OperationError> RemoveSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            Maybe<ItSystemUsage> usageResult = _usageRepository.GetByKey(itSystemUsageId);

            if (usageResult.IsNone)
                return new OperationError(OperationFailure.BadInput);

            var usage = usageResult.Value;
            if (!_authorizationContext.AllowModify(usage))
                return new OperationError(OperationFailure.Forbidden);

            return usage
                .RemoveSensitiveDataLevel(_userContext.UserEntity, sensitiveDataLevel)
                .Match<Result<ItSystemUsageSensitiveDataLevel, OperationError>>
                (
                    onSuccess: removedSensitivityLevel =>
                    {
                        _sensitiveDataLevelRepository.DeleteWithReferencePreload(removedSensitivityLevel);
                        _sensitiveDataLevelRepository.Save();
                        _usageRepository.Save();
                        return removedSensitivityLevel;
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
