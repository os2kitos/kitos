using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
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
        private readonly ILogger _logger;

        public KLEStandardRepository(
            IKLEDataBridge kleDataBridge,
            ITransactionManager transactionManager,
            IGenericRepository<TaskRef> existingTaskRefRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<TaskUsage> taskUsageRepository,
            ILogger logger): this(new KLEParentHelper(), new KLEConverterHelper(), taskUsageRepository)
        {
            _kleDataBridge = kleDataBridge;
            _transactionManager = transactionManager;
            _existingTaskRefRepository = existingTaskRefRepository;
            _systemUsageRepository = systemUsageRepository;
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
            var kleXmlData = _kleDataBridge.GetKLEXMLData();
            var publishedDate = GetPublishedDate(kleXmlData);
            return new KLEStatus
            {
                UpToDate = lastUpdated>=publishedDate.Date,
                Published = publishedDate
            };
        }

        private static DateTime GetPublishedDate(XContainer kleXmlData)
        {
            var publishedString = kleXmlData.Descendants("UdgivelsesDato").First().Value;
            var publishedDate = DateTime.Parse(publishedString, CultureInfo.GetCultureInfo("da-DK"));
            return publishedDate;
        }

        public IReadOnlyList<KLEChange> GetKLEChangeSummary()
        {
            return GetKLEChangeSummary(_kleDataBridge.GetKLEXMLData());
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
                            UpdatedDescription = mostRecentTaskRef.Description
                        });
                    } 
                    else if (existingTaskRef.Uuid == Guid.Empty)
                    {
                        result.Add(new KLEChange
                        {
                            Uuid = mostRecentTaskRef.Uuid,
                            ChangeType = KLEChangeType.UuidPatched,
                            TaskKey = existingTaskRef.TaskKey,
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
                    UpdatedDescription = mostRecentTaskRef.Description
                }));

            return result;
        }

        public DateTime UpdateKLE(int ownerObjectId, int ownedByOrgnizationUnitId)
        {
            _logger.Debug("UpdateKLE: Begin");

            var kleXmlData = _kleDataBridge.GetKLEXMLData();
            var changes = GetKLEChangeSummary(kleXmlData);
            _logger.Debug($"Changes: {changes.Count}");
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                UpdateRemovedTaskRefs(changes);
                UpdateRenamedTaskRefs(changes);
                UpdateAddedTaskRefs(changes, ownerObjectId, ownedByOrgnizationUnitId);
                PatchTaskRefUuid(changes);
                _existingTaskRefRepository.Save();
                PatchTaskRefParentId(changes);
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
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItProjects).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itProject in removedTaskRef.ItProjects)
            {
                itProject.TaskRefs.Remove(removedTaskRef);
            }
        }

        private void RemoveSystemTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystems).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itSystem in removedTaskRef.ItSystems)
            {
                itSystem.TaskRefs.Remove(removedTaskRef);
            }
        }

        // TODO: Hvad er betydningen af SystemUsageOptOut?
        private void RemoveSystemUsageOptOutTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystemUsagesOptOut).First(t => t.TaskKey == kleChange.TaskKey);
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
                systemUsage.TaskRefs.ToList().RemoveAll(t => kleChanges.Exists(c => c.TaskKey == t.TaskKey));
            }
        }

        private void RemoveTaskUsageTaskRef(List<KLEChange> kleChanges)
        {
            var taskUsages = _taskUsageRepository
                .GetWithReferencePreload(t => t.TaskRef)
                .Where(t => kleChanges.Exists(c => c.TaskKey == t.TaskRef.TaskKey));
            _taskUsageRepository.RemoveRange(taskUsages);
        }

        private void RemoveTaskRef(IEnumerable<KLEChange> kleChanges)
        {
            var removedTaskRefs = kleChanges.Select(c => _existingTaskRefRepository.Get(t => t.TaskKey == c.TaskKey).First());

            _existingTaskRefRepository.RemoveRange(removedTaskRefs);
        }

        #endregion

        #region Renames

        private void UpdateRenamedTaskRefs(IEnumerable<KLEChange> changes)
        {
            var renames = changes.Where(c => c.ChangeType == KLEChangeType.Renamed).ToList();
            _logger.Debug($"Renames: {renames.Count}");
            foreach (var kleChange in renames)
            {
                var renamedTaskRef =
                    _existingTaskRefRepository.Get(t => t.TaskKey == kleChange.TaskKey).First();
                renamedTaskRef.Description = kleChange.UpdatedDescription;
                renamedTaskRef.Uuid = kleChange.Uuid;
            }
        }

        #endregion

        #region Additions

        private void UpdateAddedTaskRefs(IEnumerable<KLEChange> changes, int ownerObjectId,
            int ownedByOrgnizationUnitId)
        {
            var additions = changes.Where(c => c.ChangeType == KLEChangeType.Added).ToList();
            _logger.Debug($"Additions: {additions.Count}");
            var addedTaskRefs = additions.Select(kleChange =>
                new TaskRef
                {
                    AccessModifier = AccessModifier.Public,
                    Uuid = kleChange.Uuid,
                    Type = kleChange.Type,
                    TaskKey = kleChange.TaskKey,
                    Description = kleChange.UpdatedDescription,
                    ObjectOwnerId = ownerObjectId,
                    LastChangedByUserId = ownerObjectId,
                    OwnedByOrganizationUnitId = ownedByOrgnizationUnitId
                }
            );
            _existingTaskRefRepository.AddRange(addedTaskRefs);
        }

        #endregion

        #region Patches

        private void PatchTaskRefUuid(IReadOnlyList<KLEChange> changes)
        {
            var patches = changes.Where(c => c.ChangeType == KLEChangeType.UuidPatched).ToList();
            foreach (var patch in patches)
            {
                var existingTaskRef = _existingTaskRefRepository.Get(t => t.TaskKey == patch.TaskKey).First();
                existingTaskRef.Uuid = patch.Uuid;
            }
        }

        private void PatchTaskRefParentId(IEnumerable<KLEChange> changes)
        {
            var additions = changes.Where(c => c.ChangeType == KLEChangeType.Added).ToList();
            foreach (var kleChange in additions)
            {
                var existingTaskRef = _existingTaskRefRepository.Get(t => t.TaskKey == kleChange.TaskKey).First();
                if (_kleParentHelper.TryDeduceParentTaskKey(kleChange.TaskKey, out var parentTaskKey))
                {
                    var parent = _existingTaskRefRepository.Get(t => t.TaskKey == parentTaskKey).First();
                    existingTaskRef.ParentId = parent.Id;
                    _logger.Debug($"Patched ParentId='{existingTaskRef.ParentId}' on '{existingTaskRef.Uuid}' as parent '{parentTaskKey}'");
                }
            }
        }

        #endregion
    }
}
