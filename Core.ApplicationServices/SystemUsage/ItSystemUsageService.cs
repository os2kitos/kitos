using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage;
using Core.ApplicationServices.Options;
using Core.DomainModel;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
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
        private readonly IGenericRepository<SystemRelation> _relationRepository;
        private readonly ILogger _logger;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemRepository systemRepository,
            IItContractRepository contractRepository,
            IOptionsService<SystemRelation, RelationFrequencyType> frequencyService,
            IOrganizationalUserContext userContext,
            IGenericRepository<SystemRelation> relationRepository,
            ITransactionManager transactionManager,
            ILogger logger)
        {
            _usageRepository = usageRepository;
            _authorizationContext = authorizationContext;
            _systemRepository = systemRepository;
            _contractRepository = contractRepository;
            _frequencyService = frequencyService;
            _userContext = userContext;
            _transactionManager = transactionManager;
            _relationRepository = relationRepository;
            _logger = logger;
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
            usage.DataLevel = newSystemUsage.DataLevel;
            usage.ContainsLegalInfo = newSystemUsage.ContainsLegalInfo;
            usage.AssociatedDataWorkers = newSystemUsage.AssociatedDataWorkers;
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
            _usageRepository.DeleteByKeyWithReferencePreload(id);
            _usageRepository.Save();
            return itSystemUsage;
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
            var fromSystemUsage = _usageRepository.GetByKey(fromSystemUsageId);
            var toSystemUsage = _usageRepository.GetByKey(toSystemUsageId);
            var toContract = Maybe<ItContract>.None;
            var targetFrequency = Maybe<RelationFrequencyType>.None;

            if (fromSystemUsage == null)
                return new OperationError("Source not found", OperationFailure.NotFound);

            if (toSystemUsage == null)
                return new OperationError("Destination could not be found", OperationFailure.BadInput);

            if (!_authorizationContext.AllowModify(fromSystemUsage))
                return Result<SystemRelation, OperationError>.Failure(OperationFailure.Forbidden);

            if (frequencyId.HasValue)
            {
                targetFrequency = _frequencyService
                    .GetAvailableOptions(fromSystemUsage.OrganizationId)
                    .FirstOrDefault(x => x.Id == frequencyId.Value);

                if (targetFrequency.IsNone)
                {
                    return new OperationError("Frequency type is not available in the organization", OperationFailure.BadInput);
                }
            }

            if (contractId.HasValue)
            {
                toContract = _contractRepository.GetById(contractId.Value);
                if (toContract.IsNone)
                {
                    return new OperationError("Contract id does not point to a valid contract", OperationFailure.BadInput);
                }
            }

            var result = fromSystemUsage.AddUsageRelationTo(_userContext.UserEntity, toSystemUsage, interfaceId, description, reference, targetFrequency, toContract);
            if (result.Ok)
            {
                _usageRepository.Save();
            }
            return result;
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
            var fromSystemUsage = _usageRepository.GetByKey(fromSystemUsageId);
            if (fromSystemUsage == null)
            {
                return new OperationError("Source not found", OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowModify(fromSystemUsage))
            {
                return Result<SystemRelation, OperationError>.Failure(OperationFailure.Forbidden);
            }

            var toSystemUsage = _usageRepository.GetByKey(toSystemUsageId);
            if (toSystemUsage == null)
            {
                return new OperationError("Target system usage not found", OperationFailure.BadInput);
            }

            var toContract = Maybe<ItContract>.None;
            if (toContractId.HasValue)
            {
                toContract = _contractRepository.GetById(toContractId.Value);
                if (toContract.IsNone)
                {
                    return new OperationError("Contract id does not point to a valid contract", OperationFailure.BadInput);
                }
            }

            var toFrequency = Maybe<RelationFrequencyType>.None;
            if (toFrequencyId.HasValue)
            {
                toFrequency = _frequencyService
                    .GetAvailableOptions(fromSystemUsage.OrganizationId)
                    .FirstOrDefault(x => x.Id == toFrequencyId.Value);

                if (toFrequency.IsNone)
                {
                    return new OperationError("Frequency type is not available in the organization", OperationFailure.BadInput);
                }
            }


            var result = fromSystemUsage.ModifyUsageRelation(_userContext.UserEntity, relationId, toSystemUsage, 
                changedDescription, changedReference, toInterfaceId, toContract, toFrequency);
            if (result.Ok)
            {
                _usageRepository.Save();
            }

            return result;
        }

        public Result<IEnumerable<SystemRelation>, OperationError> GetRelations(int fromSystemUsageId)
        {
            Maybe<ItSystemUsage> itSystemUsage = _usageRepository.GetByKey(fromSystemUsageId);
            if (itSystemUsage.HasValue == false)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            var systemUsage = itSystemUsage.Value;
            if (!_authorizationContext.AllowReads(systemUsage))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return systemUsage.UsageRelations.ToList();
        }

        public Result<SystemRelation, OperationFailure> RemoveRelation(int fromSystemUsageId, int relationId)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                Maybe<ItSystemUsage> systemUsage = _usageRepository.GetByKey(fromSystemUsageId);
                if (systemUsage.IsNone)
                {
                    return OperationFailure.NotFound;
                }

                var usage = systemUsage.Value;
                if (!_authorizationContext.AllowModify(usage))
                {
                    return OperationFailure.Forbidden;
                }

                var removalResult = usage.RemoveUsageRelation(relationId);
                if (removalResult.Failed)
                {
                    _logger.Error("Attempt to remove relation from {systemUsageId} with Id {relationId} failed with {error}", systemUsage, relationId, removalResult.Error);
                    return removalResult;
                }

                _relationRepository.DeleteWithReferencePreload(removalResult.Value);
                _relationRepository.Save();
                _usageRepository.Save();
                transaction.Commit();
                return removalResult;
            }
        }

        public Result<SystemRelation, OperationFailure> GetRelation(int fromSystemUsageId, int relationId)
        {
            Maybe<ItSystemUsage> systemUsage = _usageRepository.GetByKey(fromSystemUsageId);
            if (systemUsage.IsNone)
            {
                return OperationFailure.NotFound;
            }

            var usage = systemUsage.Value;
            if (!_authorizationContext.AllowReads(usage))
            {
                return OperationFailure.Forbidden;
            }

            return systemUsage
                .Value
                .GetUsageRelation(relationId)
                .Select<Result<SystemRelation, OperationFailure>>(relation => relation)
                .GetValueOrFallback(OperationFailure.NotFound);
        }

        public Result<IEnumerable<ItSystemUsage>, OperationError> GetSystemUsagesWhichCanBeRelatedTo(int fromSystemUsageId, Maybe<string> nameContent, int pageSize)
        {
            if (pageSize < 1)
                return new OperationError("Min page size is 1", OperationFailure.BadInput);

            if (pageSize > 25)
                return new OperationError("Max page size is 25", OperationFailure.BadInput);

            Maybe<ItSystemUsage> systemUsage = _usageRepository.GetByKey(fromSystemUsageId);
            if (systemUsage.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            var fromUsage = systemUsage.Value;
            if (!_authorizationContext.AllowReads(fromUsage))
            {
                return new OperationError("Not allowed to read target system usage", OperationFailure.Forbidden);
            }

            var accessLevel = _authorizationContext.GetOrganizationReadAccessLevel(fromUsage.OrganizationId);
            if (accessLevel < OrganizationDataReadAccessLevel.All)
            {
                return new OperationError("User does not have full READ access within the organization", OperationFailure.Forbidden);
            }

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
        }

        public Result<RelationOptionsDTO, OperationError> GetAvailableOptions(int fromSystemUsageId, int toSystemUsageId)
        {
            var source = _usageRepository.GetByKey(fromSystemUsageId);
            var destination = _usageRepository.GetByKey(toSystemUsageId);

            if (source == null)
                return new OperationError("Source not found", OperationFailure.NotFound);

            if (destination == null)
                return new OperationError("Destination could not be found", OperationFailure.BadInput);

            if (!source.IsInSameOrganizationAs(destination))
                return new OperationError("source and destination usages are from different organizations", OperationFailure.BadInput);

            if (!_authorizationContext.AllowReads(source))
                return new OperationError(OperationFailure.BadInput);

            var availableFrequencyTypes = _frequencyService.GetAvailableOptions(source.OrganizationId).ToList();
            var exposedInterfaces = destination.GetExposedInterfaces();
            var contracts = _contractRepository.GetByOrganizationId(source.OrganizationId).OrderBy(c => c.Name).ToList();

            return new RelationOptionsDTO(source, destination, exposedInterfaces, contracts, availableFrequencyTypes);
        }
    }
}
