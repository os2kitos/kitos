using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel.Organization;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.KLEDataBridge;

namespace Core.DomainServices.Repositories.KLE
{
    public class KLEStandardRepository : IKLEStandardRepository
    {
        private readonly IKLEDataBridge _kleDataBridge;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<TaskRef> _existingTaskRefRepository;

        public KLEStandardRepository(IKLEDataBridge kleDataBridge,
            ITransactionManager transactionManager,
            IGenericRepository<TaskRef> existingTaskRefRepository)
        {
            _kleDataBridge = kleDataBridge;
            _transactionManager = transactionManager;
            _existingTaskRefRepository = existingTaskRefRepository;
        }

        public KLEStatus GetKLEStatus()
        {
            var kleXmlData = _kleDataBridge.GetKLEXMLData();
            var publishedString = kleXmlData.Descendants("UdgivelsesDato").First().Value;
            var publishedDate = DateTime.Parse(publishedString, CultureInfo.InvariantCulture);
            return new KLEStatus
            {
                UpToDate = DateTime.Now.Date>=publishedDate.Date,
                Published = publishedDate
            };
        }

        public IReadOnlyList<KLEChange> GetKLEChangeSummary()
        {
            var result = new List<KLEChange>();
            var mostRecentTaskRefs = ConvertToTaskRefs(_kleDataBridge.GetKLEXMLData());
            foreach (var existingTaskRef in _existingTaskRefRepository.Get())
            {
                if (mostRecentTaskRefs.TryGet(existingTaskRef.TaskKey, out var mostRecentTaskRef))
                {
                    if (!mostRecentTaskRef.Description.Equals(existingTaskRef.Description))
                    {
                        result.Add(new KLEChange
                        {
                            Type = mostRecentTaskRef.Type,
                            ChangeType = KLEChangeType.Renamed,
                            TaskKey = existingTaskRef.TaskKey,
                            UpdatedDescription = mostRecentTaskRef.Description
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
                    Type = mostRecentTaskRef.Type,
                    ChangeType = KLEChangeType.Added,
                    TaskKey = mostRecentTaskRef.TaskKey,
                    UpdatedDescription = mostRecentTaskRef.Description
                }));

            return result;
        }

        public void UpdateKLE()
        {
            var changes = GetKLEChangeSummary();
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                foreach (var kleChange in changes)
                {
                    switch (kleChange.ChangeType)
                    {
                        case KLEChangeType.Removed:
                            RemoveProjectTaskRefs(kleChange);
                            RemoveSystemTaskRefs(kleChange);
                            RemoveSystemUsageTaskRefs(kleChange);
                            RemoveSystemUsageOptOutTaskRefs(kleChange);
                            RemoveTaskUsageTaskRef(kleChange);
                            RemoveTaskRef(kleChange);
                            break;

                        case KLEChangeType.Renamed:
                            var renamedTaskRef = _existingTaskRefRepository.Get(t => t.TaskKey == kleChange.TaskKey).First();
                            renamedTaskRef.Description = kleChange.UpdatedDescription;
                            break;

                        case KLEChangeType.Added:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                _existingTaskRefRepository.Save();
                transaction.Commit();
            }
        }

        private void RemoveProjectTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItProjects).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itProject in removedTaskRef.ItProjects)
            {
                itProject.TaskRefs.Remove(removedTaskRef);
            }

            removedTaskRef.ItProjects.Clear();
        }

        private void RemoveSystemTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystems).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itSystem in removedTaskRef.ItSystems)
            {
                itSystem.TaskRefs.Remove(removedTaskRef);
            }

            removedTaskRef.ItSystems.Clear();
        }

        private void RemoveSystemUsageTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystemUsages).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itSystemUsage in removedTaskRef.ItSystemUsages)
            {
                itSystemUsage.TaskRefs.Remove(removedTaskRef);
            }

            removedTaskRef.ItSystemUsages.Clear();
        }

        private void RemoveSystemUsageOptOutTaskRefs(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItSystemUsagesOptOut).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var itSystemUsageOptOut in removedTaskRef.ItSystemUsagesOptOut)
            {
                itSystemUsageOptOut.TaskRefs.Remove(removedTaskRef);
            }

            removedTaskRef.ItSystemUsagesOptOut.Clear();
        }

        private void RemoveTaskUsageTaskRef(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.Usages).First(t => t.TaskKey == kleChange.TaskKey);
            foreach (var taskUsage in removedTaskRef.Usages)
            {
                taskUsage.TaskRef = null;
            }

            removedTaskRef.Usages.Clear();
        }

        private void RemoveTaskRef(KLEChange kleChange)
        {
            var removedTaskRef =
                _existingTaskRefRepository.GetWithReferencePreload(t => t.ItProjects).First(t => t.TaskKey == kleChange.TaskKey);

            _existingTaskRefRepository.Delete(removedTaskRef);
        }

        private static MostRecentKLE ConvertToTaskRefs(XDocument document)
        {
            var result = new MostRecentKLE();
            result.AddRange(document.Descendants("Hovedgruppe").Select(mainGroup => 
                new TaskRef
                {
                    TaskKey = mainGroup.Descendants("HovedgruppeNr").First().Value,
                    Description = mainGroup.Descendants("HovedgruppeTitel").First().Value,
                    Type = "KLE-Hovedgruppe"
                }));
            result.AddRange(document.Descendants("Gruppe").Select(mainGroup =>
                new TaskRef
                {
                    TaskKey = mainGroup.Descendants("GruppeNr").First().Value,
                    Description = mainGroup.Descendants("GruppeTitel").First().Value,
                    Type = "KLE-Gruppe"
                }));
            result.AddRange(document.Descendants("Emne").Select(mainGroup =>
                new TaskRef
                {
                    TaskKey = mainGroup.Descendants("EmneNr").First().Value,
                    Description = mainGroup.Descendants("EmneTitel").First().Value,
                    Type = "KLE-Emne"
                }));
            return result;
        }
    }
}
