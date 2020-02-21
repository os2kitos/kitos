using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
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
        private readonly IGenericRepository<TaskUsage> _taskUsageRepository;
        private readonly IKLEParentHelper _kleParentHelper;
        private readonly IKLEConverterHelper _kleConverterHelper;
        private readonly IOperationClock _clock;
        private readonly ILogger _logger;

        public KLEStandardRepository(
            IKLEDataBridge kleDataBridge,
            ITransactionManager transactionManager,
            IGenericRepository<TaskRef> existingTaskRefRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<TaskUsage> taskUsageRepository,
            IOperationClock clock,
            ILogger logger) : this(new KLEParentHelper(), new KLEConverterHelper(clock), taskUsageRepository)
        {
            _kleDataBridge = kleDataBridge;
            _transactionManager = transactionManager;
            _existingTaskRefRepository = existingTaskRefRepository;
            _systemUsageRepository = systemUsageRepository;
            _clock = clock;
            _logger = logger;
        }

        private KLEStandardRepository(IKLEParentHelper kleParentHelper, IKLEConverterHelper kleConverterHelper, IGenericRepository<TaskUsage> taskUsageRepository)
        {
            _kleParentHelper = kleParentHelper;
            _kleConverterHelper = kleConverterHelper;
            _taskUsageRepository = taskUsageRepository;
        }

        public KLEStatus GetKLEStatus(DateTime lastUpdated)
        {
            var kleXmlData = _kleDataBridge.GetAllActiveKleNumbers();
            var publishedDate = GetPublishedDate(kleXmlData);
            return new KLEStatus
            {
                UpToDate = lastUpdated >= publishedDate.Date,
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
                    else if (existingTaskRef.Uuid == Guid.Empty)
                    {
                        result.Add(new KLEChange
                        {
                            Uuid = mostRecentTaskRef.Uuid,
                            ChangeType = KLEChangeType.UuidPatched,
                            TaskKey = existingTaskRef.TaskKey,
                            ChangeDetails = "Opdatering af null uuid",
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

        public DateTime UpdateKLE(int ownerObjectId, int ownedByOrgnizationUnitId)
        {
            _logger.Debug("UpdateKLE: Begin");

            var kleXmlData = _kleDataBridge.GetAllActiveKleNumbers();
            var changes = GetKLEChangeSummary(kleXmlData);
            _logger.Debug($"Changes: {changes.Count}");
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                var updateTime = _clock.Now;

                // Changes first run
                UpdateRemovedTaskRefs(changes);
                UpdateRenamedTaskRefs(changes, ownerObjectId, ownedByOrgnizationUnitId, updateTime);
                UpdateAddedTaskRefs(changes, ownerObjectId, ownedByOrgnizationUnitId, updateTime);
                PatchTaskRefUuid(changes, ownerObjectId, ownedByOrgnizationUnitId, updateTime);
                _existingTaskRefRepository.Save();

                // Changes second run, takes into account removed/added items
                PatchTaskRefParentId(changes, ownerObjectId, ownedByOrgnizationUnitId, updateTime);
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
                RemoveProjectTaskRefs(kleChange);
                RemoveSystemTaskRefs(kleChange);
                RemoveSystemUsageOptOutTaskRefs(kleChange);
            }
            RemoveSystemUsageTaskRefs(removals);
            RemoveTaskUsageTaskRef(removals);
            RemoveTaskRef(removals);
        }

        private void RemoveProjectTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef = _existingTaskRefRepository.GetWithReferencePreload(t => t.ItProjects).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itProject in removedTaskRef.ItProjects)
            {
                itProject.TaskRefs.Remove(removedTaskRef);
            }
        }

        private void RemoveSystemTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef = _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystems).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itSystem in removedTaskRef.ItSystems)
            {
                itSystem.TaskRefs.Remove(removedTaskRef);
            }
        }

        private void RemoveSystemUsageOptOutTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef = _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystemUsagesOptOut).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itSystemUsageOptOut in removedTaskRef.ItSystemUsagesOptOut)
            {
                itSystemUsageOptOut.TaskRefs.Remove(removedTaskRef);
            }
        }

        private void RemoveSystemUsageTaskRefs(List<KLEChange> kleChanges)
        {
            var systemUsages = _systemUsageRepository.GetWithReferencePreload(s => s.TaskRefs);
            foreach (var systemUsage in systemUsages)
            {
                foreach (var taskRef in systemUsage.TaskRefs.ToList().Where(t => kleChanges.Exists(c => c.TaskKey == t.TaskKey)))
                {
                    systemUsage.TaskRefs.Remove(taskRef);
                }
            }
        }

        private void RemoveTaskUsageTaskRef(IEnumerable<KLEChange> kleChanges)
        {
            var keys = kleChanges.Select(x=>x.TaskKey).ToList();
            var taskUsages = _taskUsageRepository
                .GetWithReferencePreload(t => t.TaskRef)
                .Where(t => keys.Contains(t.TaskRef.TaskKey))
                .ToList();

            _taskUsageRepository.RemoveRange(taskUsages);
        }

        private void RemoveTaskRef(IEnumerable<KLEChange> kleChanges)
        {
            var removedTaskRefs = kleChanges.Select(c => _existingTaskRefRepository.Get(t => t.TaskKey == c.TaskKey).First());

            _existingTaskRefRepository.RemoveRange(removedTaskRefs);
        }

        #endregion

        #region Renames

        private void UpdateRenamedTaskRefs(IEnumerable<KLEChange> changes, int ownerObjectId,
            int ownedByOrgnizationUnitId, DateTime updateTime)
        {
            var updates = BuildChangeSet(changes, KLEChangeType.Renamed);
            foreach (var update in updates)
            {
                update.Item1.Description = update.Item2.UpdatedDescription;
                update.Item1.Uuid = update.Item2.Uuid;
                update.Item1.LastChanged = updateTime;
                update.Item1.LastChangedByUserId = ownerObjectId;
                update.Item1.OwnedByOrganizationUnitId = ownedByOrgnizationUnitId;
                update.Item1.ActiveFrom = update.Item2.ActiveFrom;
                update.Item1.ActiveTo = update.Item2.ActiveTo;
            }
        }

        #endregion

        #region Additions

        private void UpdateAddedTaskRefs(IEnumerable<KLEChange> changes, int ownerObjectId,
            int ownedByOrgnizationUnitId, DateTime updateTime)
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
                    ObjectOwnerId = ownerObjectId,
                    LastChanged = updateTime,
                    LastChangedByUserId = ownerObjectId,
                    OwnedByOrganizationUnitId = ownedByOrgnizationUnitId,
                    ActiveFrom = kleChange.ActiveFrom,
                    ActiveTo = kleChange.ActiveTo,
                }
            );
            _existingTaskRefRepository.AddRange(addedTaskRefs);
        }

        #endregion

        #region Patches

        private void PatchTaskRefUuid(IEnumerable<KLEChange> changes, int ownerObjectId,
            int ownedByOrgnizationUnitId, DateTime updateTime)
        {
            var updates = BuildChangeSet(changes, KLEChangeType.UuidPatched).ToList();
            _logger.Debug($"Patch-uuids: {updates.Count}");
            foreach (var update in updates)
            {
                update.Item1.Uuid = update.Item2.Uuid;
                update.Item1.LastChanged = updateTime;
                update.Item1.LastChangedByUserId = ownerObjectId;
                update.Item1.OwnedByOrganizationUnitId = ownedByOrgnizationUnitId;
                update.Item1.ActiveFrom = update.Item2.ActiveFrom;
                update.Item1.ActiveTo = update.Item2.ActiveTo;
            }
        }

        private void PatchTaskRefParentId(IEnumerable<KLEChange> changes, int ownerObjectId,
            int ownedByOrgnizationUnitId, DateTime updateTime)
        {
            var updates = BuildChangeSet(changes, KLEChangeType.Added);

            foreach (var update in updates)
            {
                var existingTaskRef = update.Item1;
                if (_kleParentHelper.TryDeduceParentTaskKey(update.Item2.TaskKey, out var parentTaskKey))
                {
                    var parent = _existingTaskRefRepository.Get(t => t.TaskKey == parentTaskKey).First();
                    existingTaskRef.ParentId = parent.Id;
                    existingTaskRef.LastChanged = updateTime;
                    existingTaskRef.LastChangedByUserId = ownerObjectId;
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
                .Select(taskRef => new Tuple<TaskRef, KLEChange>(taskRef, kleChanges[taskRef.TaskKey]));
            return updates;
        }

        #endregion
    }
}
