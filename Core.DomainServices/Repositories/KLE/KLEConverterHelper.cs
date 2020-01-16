using System;
using System.Linq;
using System.Xml.Linq;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Repositories.KLE
{
    public class KLEConverterHelper : IKLEConverterHelper
    {
        public KLEMostRecent ConvertToTaskRefs(XDocument document)
        {
            var result = new KLEMostRecent();
            result.AddRange(document.Descendants("Hovedgruppe").Select(mainGroup => 
                new TaskRef
                {
                    Uuid = Guid.Parse(mainGroup.Elements("UUID").First().Value),
                    TaskKey = mainGroup.Elements("HovedgruppeNr").First().Value,
                    Description = mainGroup.Elements("HovedgruppeTitel").First().Value,
                    Type = "KLE-Hovedgruppe"
                }));
            result.AddRange(document.Descendants("Gruppe").Select(@group =>
                new TaskRef
                {
                    Uuid = Guid.Parse(@group.Elements("UUID").First().Value),
                    TaskKey = @group.Elements("GruppeNr").First().Value,
                    Description = @group.Elements("GruppeTitel").First().Value,
                    Type = "KLE-Gruppe"
                }));
            result.AddRange(document.Descendants("Emne").Select(item =>
                new TaskRef
                {
                    Uuid = Guid.Parse(item.Elements("UUID").First().Value),
                    TaskKey = item.Elements("EmneNr").First().Value,
                    Description = item.Elements("EmneTitel").First().Value,
                    Type = "KLE-Emne"
                }));
            return result;
        }
    }
}