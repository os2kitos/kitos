using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.KLEDataBridge;

using Serilog;

namespace Core.DomainServices.Repositories.KLE
{
    public class KLEStandardRepository : IKLEStandardRepository
    {
        private readonly IKLEDataBridge _kleDataBridge;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<TaskRef> _existingTaskRefRepository;
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IKLEParentHelper _kleParentHelper;
        private readonly IKLEConverterHelper _kleConverterHelper;
        private readonly ILogger _logger;
        private readonly IDomainEvents _domainEvents;

        public KLEStandardRepository(
            IKLEDataBridge kleDataBridge,
            ITransactionManager transactionManager,
            IGenericRepository<TaskRef> existingTaskRefRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IOperationClock clock,
            ILogger logger,
            IDomainEvents domainEvents) : this(new KLEParentHelper(), new KLEConverterHelper(clock))
        {
            _kleDataBridge = kleDataBridge;
            _transactionManager = transactionManager;
            _existingTaskRefRepository = existingTaskRefRepository;
            _systemUsageRepository = systemUsageRepository;
            _logger = logger;
            _domainEvents = domainEvents;
        }

        private KLEStandardRepository(IKLEParentHelper kleParentHelper, IKLEConverterHelper kleConverterHelper)
        {
            _kleParentHelper = kleParentHelper;
            _kleConverterHelper = kleConverterHelper;
        }

        public KLEStatus GetKLEStatus(Maybe<DateTime> lastUpdated)
        {
            var kleXmlData = _kleDataBridge.GetAllActiveKleNumbers();
            var publishedDate = GetPublishedDate(kleXmlData);
            return new KLEStatus
            {
                UpToDate = lastUpdated
                    .Select(updatedAt => updatedAt.Date >= publishedDate.Date)
                    .GetValueOrFallback(false),
                Published = publishedDate
            };
        }

        private static DateTime GetPublishedDate(XContainer kleXmlData)
        {
            var publishedString = kleXmlData.Descendants("UdgivelsesDato").First().Value;
            var publishedDate = DateTime.Parse(publishedString, CultureInfo.GetCultureInfo("da-DK"));
            return publishedDate;
        }

        public IOrderedEnumerable<KLEChange> GetKLEChangeSummary()
        {
            var kleXmlData = _kleDataBridge.GetAllActiveKleNumbers();

            var kleChangeSummary = GetKLEChangeSummary(kleXmlData);

            return kleChangeSummary.OrderBy(c => c.TaskKey);
        }

        private IReadOnlyList<KLEChange> GetKLEChangeSummary(XDocument kleXmlData)
        {
            var result = new List<KLEChange>();
            var mostRecentTaskRefs = _kleConverterHelper.ConvertToTaskRefs(kleXmlData);

            foreach (var existingTaskRef in _existingTaskRefRepository.Get())
            {
                if (mostRecentTaskRefs.TryGet(existingTaskRef.TaskKey, out var mostRecentTaskRef))
                {
                    if (!mostRecentTaskRef.Description.Equals(existingTaskRef.Description))
                    {
                        result.Add(new KLEChange
                        {
                            Uuid = mostRecentTaskRef.Uuid,
                            Type = mostRecentTaskRef.Type,
                            ChangeType = KLEChangeType.Renamed,
                            TaskKey = existingTaskRef.TaskKey,
                            UpdatedDescription = mostRecentTaskRef.Description,
                            ChangeDetails = $"Navneskift fra '{existingTaskRef.Description}' til '{mostRecentTaskRef.Description}'",
                            ActiveTo = mostRecentTaskRef.ActiveTo,
                            ActiveFrom = mostRecentTaskRef.ActiveFrom,
                        });
                    }
                    else if (existingTaskRef.Uuid != mostRecentTaskRef.Uuid)
                    {
                        result.Add(new KLEChange
                        {
                            Uuid = mostRecentTaskRef.Uuid,
                            ChangeType = KLEChangeType.UuidPatched,
                            TaskKey = existingTaskRef.TaskKey,
                            ChangeDetails = "Opdatering af uuid",
                            ActiveTo = mostRecentTaskRef.ActiveTo,
                            ActiveFrom = mostRecentTaskRef.ActiveFrom,
                        });
                    }
                    mostRecentTaskRefs.Remove(mostRecentTaskRef.TaskKey);
                }
                else
                {
                    result.Add(new KLEChange
                    {
                        Type = existingTaskRef.Type,
                        ChangeType = KLEChangeType.Removed,
                        TaskKey = existingTaskRef.TaskKey,
                        UpdatedDescription = existingTaskRef.Description
                    });
                }
            }
            result.AddRange(mostRecentTaskRefs.GetAll().Select(mostRecentTaskRef =>
                new KLEChange
                {
                    Uuid = mostRecentTaskRef.Uuid,
                    Type = mostRecentTaskRef.Type,
                    ChangeType = KLEChangeType.Added,
                    TaskKey = mostRecentTaskRef.TaskKey,
                    UpdatedDescription = mostRecentTaskRef.Description,
                    ChangeDetails = "Nyt KLE element",
                    ActiveTo = mostRecentTaskRef.ActiveTo,
                    ActiveFrom = mostRecentTaskRef.ActiveFrom,
                }));

            return result;
        }

        public DateTime UpdateKLE(int ownedByOrgnizationUnitId)
        {
            _logger.Debug("UpdateKLE: Begin");

            var kleXmlData = _kleDataBridge.GetAllActiveKleNumbers();
            var changes = GetKLEChangeSummary(kleXmlData);
            _logger.Debug($"Changes: {changes.Count}");
            using (var transaction = _transactionManager.Begin())
            {
                // Changes first run
                UpdateRemovedTaskRefs(changes);
                UpdateRenamedTaskRefs(changes, ownedByOrgnizationUnitId);
                UpdateAddedTaskRefs(changes, ownedByOrgnizationUnitId);
                PatchTaskRefUuid(changes, ownedByOrgnizationUnitId);
                _existingTaskRefRepository.Save();

                // Changes second run, takes into account removed/added items
                PatchTaskRefParentId(changes, ownedByOrgnizationUnitId);
                _existingTaskRefRepository.Save();

                transaction.Commit();
            }
            _logger.Debug("UpdateKLE: End");

            return GetPublishedDate(kleXmlData);
        }

        #region Removals

        private void UpdateRemovedTaskRefs(IEnumerable<KLEChange> changes)
        {
            var removals = changes.Where(c => c.ChangeType == KLEChangeType.Removed).ToList();
            _logger.Debug($"Removals: {removals.Count}");
            foreach (var kleChange in removals)
            {
                RemoveSystemTaskRefs(kleChange);
                RemoveSystemUsageOptOutTaskRefs(kleChange);
            }
            RemoveSystemUsageTaskRefs(removals);
            RemoveTaskRef(removals);
        }

        private void RemoveSystemTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef = _existingTaskRefRepository
                .GetWithReferencePreload(t => t.ItSystems)
                .First(t => t.TaskKey == kleChange.TaskKey);

            foreach (var itSystem in removedTaskRef.ItSystems.ToList())
            {
                itSystem.TaskRefs.Remove(removedTaskRef);
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(itSystem));
            }
        }

        private void RemoveSystemUsageOptOutTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef = _existingTaskRefRepository
                .GetWithReferencePreload(t => t.ItSystemUsagesOptOut)
                .First(t => t.TaskKey == kleChange.TaskKey);

            foreach (var itSystemUsageOptOut in removedTaskRef.ItSystemUsagesOptOut.ToList())
            {
                itSystemUsageOptOut.TaskRefs.Remove(removedTaskRef);
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(itSystemUsageOptOut));
            }
        }

        private void RemoveSystemUsageTaskRefs(List<KLEChange> kleChanges)
        {
            var removedTaskKeys = kleChanges.Select(x => x.TaskKey).ToHashSet();

            var affectedSystemUsages = _systemUsageRepository
                .GetWithReferencePreload(s => s.TaskRefs)
                .Where(system => system.TaskRefs.Any(tr => removedTaskKeys.Contains(tr.TaskKey)))
                .ToList();

            foreach (var systemUsage in affectedSystemUsages)
            {
                foreach (var taskRef in systemUsage.TaskRefs.Where(t => removedTaskKeys.Contains(t.TaskKey)).ToList())
                {
                    systemUsage.TaskRefs.Remove(taskRef);
                }
                _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(systemUsage));
            }
        }

        private void RemoveTaskRef(IEnumerable<KLEChange> kleChanges)
        {
            var removedTaskKeys = kleChanges
                .Select(x => x.TaskKey)
                .ToList();

            var removedTaskRefs = _existingTaskRefRepository
                .AsQueryable()
                .Where(tr => removedTaskKeys.Contains(tr.TaskKey))
                .ToList();

            _existingTaskRefRepository.RemoveRange(removedTaskRefs);
        }

        #endregion

        #region Renames

        private void UpdateRenamedTaskRefs(IEnumerable<KLEChange> changes,
            int ownedByOrgnizationUnitId)
        {
            var updates = BuildChangeSet(changes, KLEChangeType.Renamed);
            foreach (var update in updates.ToList())
            {
                update.Item1.Description = update.Item2.UpdatedDescription;
                update.Item1.Uuid = update.Item2.Uuid;
                update.Item1.OwnedByOrganizationUnitId = ownedByOrgnizationUnitId;
                update.Item1.ActiveFrom = update.Item2.ActiveFrom;
                update.Item1.ActiveTo = update.Item2.ActiveTo;
                _domainEvents.Raise(new EntityUpdatedEvent<TaskRef>(update.Item1));
            }
        }

        #endregion

        #region Additions

        private void UpdateAddedTaskRefs(IEnumerable<KLEChange> changes,
            int ownedByOrganizationUnitId)
        {
            var additions = changes.Where(c => c.ChangeType == KLEChangeType.Added).ToList();
            _logger.Debug($"Additions: {additions.Count}");
            var addedTaskRefs = additions.Select(kleChange =>
                new TaskRef
                {
                    Uuid = kleChange.Uuid,
                    Type = kleChange.Type,
                    TaskKey = kleChange.TaskKey,
                    Description = kleChange.UpdatedDescription,
                    OwnedByOrganizationUnitId = ownedByOrganizationUnitId,
                    ActiveFrom = kleChange.ActiveFrom,
                    ActiveTo = kleChange.ActiveTo,
                }
            );
            _existingTaskRefRepository.AddRange(addedTaskRefs);
        }

        #endregion

        #region Patches

        private void PatchTaskRefUuid(IEnumerable<KLEChange> changes,
            int ownedByOrgnizationUnitId)
        {
            var updates = BuildChangeSet(changes, KLEChangeType.UuidPatched).ToList();
            _logger.Debug($"Patch-uuids: {updates.Count}");
            foreach (var update in updates)
            {
                update.Item1.Uuid = update.Item2.Uuid;
                update.Item1.OwnedByOrganizationUnitId = ownedByOrgnizationUnitId;
                update.Item1.ActiveFrom = update.Item2.ActiveFrom;
                update.Item1.ActiveTo = update.Item2.ActiveTo;
            }
        }

        private void PatchTaskRefParentId(IEnumerable<KLEChange> changes,
            int ownedByOrgnizationUnitId)
        {
            var updates = BuildChangeSet(changes, KLEChangeType.Added);

            foreach (var update in updates.ToList())
            {
                var existingTaskRef = update.Item1;
                if (_kleParentHelper.TryDeduceParentTaskKey(update.Item2.TaskKey, out var parentTaskKey))
                {
                    var parent = _existingTaskRefRepository.Get(t => t.TaskKey == parentTaskKey).FirstOrDefault();
                    if (parent == null) continue;
                    existingTaskRef.ParentId = parent.Id;
                    existingTaskRef.OwnedByOrganizationUnitId = ownedByOrgnizationUnitId;
                }
            }
        }

        private IEnumerable<Tuple<TaskRef, KLEChange>> BuildChangeSet(IEnumerable<KLEChange> changes, KLEChangeType kleChangeType)
        {
            var changesByType = changes.Where(c => c.ChangeType == kleChangeType).ToList();

            var kleChanges = changesByType.ToDictionary(change => change.TaskKey);
            var taskKeys = changesByType.Select(x => x.TaskKey).ToList();

            var updates = _existingTaskRefRepository
                .AsQueryable()
                .Where(taskRef => taskKeys.Contains(taskRef.TaskKey))
                .AsEnumerable()
                .Select(taskRef => new Tuple<TaskRef, KLEChange>(taskRef, kleChanges[taskRef.TaskKey]))
                .ToList();
            return updates;
        }

        #endregion
    }
}
