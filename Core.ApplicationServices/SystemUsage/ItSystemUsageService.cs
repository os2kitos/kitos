using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItSystemRepository _systemRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<ItSystemUsageSensitiveDataLevel> _sensitiveDataLevelRepository;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IItSystemUsageAttachedOptionRepository _itSystemUsageAttachedOptionRepository;
        private readonly IReferenceService _referenceService;
        private readonly IGenericRepository<ArchivePeriod> _archivePeriodRepository;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemRepository systemRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IGenericRepository<ItSystemUsageSensitiveDataLevel> sensitiveDataLevelRepository,
            IOrganizationalUserContext userContext,
            IItSystemUsageAttachedOptionRepository itSystemUsageAttachedOptionRepository, 
            IGenericRepository<ArchivePeriod> archivePeriodRepository)
        {
            _usageRepository = usageRepository;
            _authorizationContext = authorizationContext;
            _systemRepository = systemRepository;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _referenceService = referenceService;
            _sensitiveDataLevelRepository = sensitiveDataLevelRepository;
            _userContext = userContext;
            _itSystemUsageAttachedOptionRepository = itSystemUsageAttachedOptionRepository;
            _archivePeriodRepository = archivePeriodRepository;
        }

        public IQueryable<ItSystemUsage> Query(params IDomainQuery<ItSystemUsage>[] conditions)
        {
            var baseQuery = _usageRepository.AsQueryable();
            var subQueries = new List<IDomainQuery<ItSystemUsage>>();

            if (_authorizationContext.GetCrossOrganizationReadAccess() < CrossOrganizationDataReadAccessLevel.All)
                subQueries.Add(new QueryByOrganizationIds<ItSystemUsage>(_userContext.OrganizationIds));

            subQueries.AddRange(conditions);

            var result = subQueries.Any()
                ? new IntersectionQuery<ItSystemUsage>(subQueries).Apply(baseQuery)
                : baseQuery;

            return result;
        }

        public Result<ItSystemUsage, OperationError> CreateNew(int itSystemId, int organizationId)
        {
            var input = new ItSystemUsage
            {
                ItSystemId = itSystemId,
                OrganizationId = organizationId
            };
            return Add(input);
        }

        public Result<ItSystemUsage, OperationError> Add(ItSystemUsage newSystemUsage)
        {
            if (newSystemUsage == null)
                throw new ArgumentNullException(nameof(newSystemUsage));

            // create the system usage
            var existing = GetByOrganizationAndSystemId(newSystemUsage.OrganizationId, newSystemUsage.ItSystemId);
            if (existing != null)
            {
                return new OperationError("Only one system usage per it-system is allowed per organization", OperationFailure.Conflict);
            }

            if (!_authorizationContext.AllowCreate<ItSystemUsage>(newSystemUsage.OrganizationId, newSystemUsage))
            {
                return new OperationError("User is not allowed to create itsystem usages", OperationFailure.Forbidden);
            }

            var itSystem = _systemRepository.GetSystem(newSystemUsage.ItSystemId);
            if (itSystem == null)
            {
                return new OperationError("System not found", OperationFailure.BadInput);
            }

            if (!_authorizationContext.AllowReads(itSystem))
            {
                return new OperationError("User does not have access to the target system", OperationFailure.Forbidden);
            }

            //Cannot create system usage in an org where the logical it system is unavailable to the users.
            if (!AllowUsageInTargetOrganization(newSystemUsage, itSystem))
            {
                return new OperationError("Users in target organization do not have access to the system", OperationFailure.Forbidden);
            }

            var usage = _usageRepository.Create();

            usage.ItSystemId = newSystemUsage.ItSystemId;
            usage.OrganizationId = newSystemUsage.OrganizationId;
            _usageRepository.Insert(usage);
            _usageRepository.Save(); // abuse this as UoW
            _domainEvents.Raise(new EntityCreatedEvent<ItSystemUsage>(usage));

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

                _itSystemUsageAttachedOptionRepository.DeleteAllBySystemUsageId(id);

                _domainEvents.Raise(new EntityDeletedEvent<ItSystemUsage>(itSystemUsage));
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

        public Result<ItSystemUsage, OperationError> GetByUuid(Guid uuid)
        {
            return _usageRepository
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Match<Result<ItSystemUsage, OperationError>>
                (
                    systemUsage => _authorizationContext.AllowReads(systemUsage) ? systemUsage : new OperationError(OperationFailure.Forbidden),
                    () => new OperationError(OperationFailure.NotFound)
                );
        }

        public Result<ItSystemUsageSensitiveDataLevel, OperationError> AddSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            Maybe<ItSystemUsage> usageResult = _usageRepository.GetByKey(itSystemUsageId);

            if (usageResult.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var usage = usageResult.Value;
            if (!_authorizationContext.AllowModify(usage))
                return new OperationError(OperationFailure.Forbidden);

            return usage
                .AddSensitiveDataLevel(sensitiveDataLevel)
                .Match<Result<ItSystemUsageSensitiveDataLevel, OperationError>>
                (
                    onSuccess: addedSensitivityLevel =>
                    {
                        _usageRepository.Save();
                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
                        return addedSensitivityLevel;
                    },
                    onFailure: error => error);
        }

        public Result<ItSystemUsageSensitiveDataLevel, OperationError> RemoveSensitiveDataLevel(int itSystemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            Maybe<ItSystemUsage> usageResult = _usageRepository.GetByKey(itSystemUsageId);

            if (usageResult.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var usage = usageResult.Value;
            if (!_authorizationContext.AllowModify(usage))
                return new OperationError(OperationFailure.Forbidden);

            return usage
                .RemoveSensitiveDataLevel(sensitiveDataLevel)
                .Match<Result<ItSystemUsageSensitiveDataLevel, OperationError>>
                (
                    onSuccess: removedSensitivityLevel =>
                    {
                        _sensitiveDataLevelRepository.DeleteWithReferencePreload(removedSensitivityLevel);
                        _sensitiveDataLevelRepository.Save();
                        _usageRepository.Save();
                        _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
                        return removedSensitivityLevel;
                    },
                    onFailure: error =>
                        error.FailureType == OperationFailure.NotFound
                            ? new OperationError(OperationFailure.BadInput)
                            : error);
        }

        public Result<IEnumerable<ArchivePeriod>, OperationError> RemoveAllArchivePeriods(int systemUsageId)
        {
            return Modify(systemUsageId, usage =>
            {
                var removedPeriods = usage
                    .ResetArchivePeriods()
                    .Match<Result<IEnumerable<ArchivePeriod>, OperationError>>
                    (
                        periodsToRemove =>
                        {
                            var removed = periodsToRemove.ToList();
                            foreach (var removedPeriod in removed)
                            {
                                _archivePeriodRepository.DeleteWithReferencePreload(removedPeriod);
                            }
                            _archivePeriodRepository.Save();
                            return removed;
                        },
                        error => error);

                return removedPeriods.Failed ? removedPeriods.Error : removedPeriods;
            });
        }

        public Result<ArchivePeriod, OperationError> AddArchivePeriod(int systemUsageId, DateTime startDate, DateTime endDate, string archiveId, bool approved)
        {
            return Modify(systemUsageId, usage => usage.AddArchivePeriod(startDate, endDate, archiveId, approved));
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(int id, Func<ItSystemUsage, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var usage = _usageRepository.GetByKey(id);

            if (usage == null)
                return new OperationError(OperationFailure.NotFound);

            if (!_authorizationContext.AllowModify(usage))
                return new OperationError(OperationFailure.Forbidden);

            var mutationResult = mutation(usage);

            if (mutationResult.Ok)
            {
                _usageRepository.Update(usage);
                _usageRepository.Save();
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
                transaction.Commit();
            }

            return mutationResult;
        }
    }
}
