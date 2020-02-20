using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.Result;

namespace Core.DomainServices.Repositories.KLE
{
    public class KLEConverterHelper : IKLEConverterHelper
    {
        private readonly IOperationClock _operationClock;

        public KLEConverterHelper(IOperationClock operationClock)
        {
            _operationClock = operationClock;
        }

        public KLEMostRecent ConvertToTaskRefs(XDocument document)
        {
            var now = _operationClock.Now;
            var result = new KLEMostRecent();
            var mainGroups = document.Descendants("Hovedgruppe").Select(mainGroup =>
                new TaskRef
                {
                    Uuid = TryParseUUID(mainGroup).GetValueOrFallback(Guid.Empty),
                    TaskKey = mainGroup.Elements("HovedgruppeNr").First().Value,
                    Description = mainGroup.Elements("HovedgruppeTitel").First().Value,
                    Type = "KLE-Hovedgruppe",
                    ActiveFrom = ParseActiveFrom(mainGroup, "HovedgruppeAdministrativInfo"),
                    ActiveTo = ParseActiveTo(mainGroup, "HovedgruppeAdministrativInfo")
                });

            var subGroups = document.Descendants("Gruppe").Select(@group =>
                new TaskRef
                {
                    Uuid = TryParseUUID(@group).GetValueOrFallback(Guid.Empty),
                    TaskKey = @group.Elements("GruppeNr").First().Value,
                    Description = @group.Elements("GruppeTitel").First().Value,
                    Type = "KLE-Gruppe",
                    ActiveFrom = ParseActiveFrom(@group, "GruppeAdministrativInfo"),
                    ActiveTo = ParseActiveTo(@group, "GruppeAdministrativInfo")
                });

            var subjects = document.Descendants("Emne").Select(item =>
                new TaskRef
                {
                    Uuid = TryParseUUID(item).GetValueOrFallback(Guid.Empty),
                    TaskKey = item.Elements("EmneNr").First().Value,
                    Description = item.Elements("EmneTitel").First().Value,
                    Type = "KLE-Emne",
                    ActiveFrom = ParseActiveFrom(item, "EmneAdministrativInfo"),
                    ActiveTo = ParseActiveTo(item, "EmneAdministrativInfo")
                });

            result.AddRange(ApplyFiltering(mainGroups, now));
            result.AddRange(ApplyFiltering(subGroups, now));
            result.AddRange(ApplyFiltering(subjects, now));

            return result;
        }

        private static IEnumerable<TaskRef> ApplyFiltering(IEnumerable<TaskRef> initialTaskRefs, DateTime now)
        {
            var withoutObsoleted = RemoveObsoleted(initialTaskRefs, now);

            foreach (var taskRefs in withoutObsoleted.GroupBy(x => x.TaskKey))
            {
                var taskRefGroup = taskRefs.ToList();
                if (taskRefGroup.Count == 1)
                {
                    //No overlaps
                    yield return taskRefGroup[0];
                }
                else
                {
                    yield return PickActiveTaskRef(taskRefGroup);
                }
            }
        }

        private static IEnumerable<TaskRef> RemoveObsoleted(IEnumerable<TaskRef> mainGroups, DateTime now)
        {
            return mainGroups.Where(x => x.ActiveTo == null || x.ActiveTo > now);
        }

        private static TaskRef PickActiveTaskRef(IReadOnlyCollection<TaskRef> taskRefGroup)
        {
            var mostRecentWithoutEndDate =
                taskRefGroup
                    .Where(x => x.ActiveTo == null)
                    .OrderByDescending(x => x.ActiveFrom)
                    .FirstOrDefault();

            //Pick the most recently created which has not been obsoleted OR pick the most recently created
            return mostRecentWithoutEndDate ??
                   taskRefGroup
                       .OrderByDescending(x => x.ActiveFrom)
                       .First();
        }

        private static DateTime? ParseActiveFrom(XElement element, string infoBlockName)
        {
            var date = element
                .Elements(infoBlockName)
                .FirstOrDefault()?
                .Elements("OprettetDato")
                .FirstOrDefault()?
                .Value;
            return date == null ? default(DateTime?) : DateTime.Parse(date);
        }

        private static DateTime? ParseActiveTo(XElement element, string infoBlockName)
        {
            var date = element
                .Elements(infoBlockName)
                .FirstOrDefault()?
                .Elements("Historisk")
                .FirstOrDefault()?
                .Elements("UdgaaetDato")
                .FirstOrDefault()?
                .Value;

            return date == null ? default(DateTime?) : DateTime.Parse(date);
        }

        private static Maybe<Guid> TryParseUUID(XElement element)
        {
            //Some times the UUID is empty from KL so we cannot count on it being valid
            return Guid.TryParse(element.Elements("UUID").FirstOrDefault()?.Value ?? string.Empty, out var uuid)
                ? Maybe<Guid>.Some(uuid)
                : Maybe<Guid>.None;
        }
    }
}