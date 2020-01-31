using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage
{
    public class RelationOptionsDTO
    {
        public ItSystemUsage FromSystemUsage { get; }
        public ItSystemUsage ToSystemUsage { get; }
        public IEnumerable<ItInterface> AvailableInterfaces { get; }
        public IEnumerable<ItContract> AvailableContracts { get; }
        public IEnumerable<RelationFrequencyType> AvailableFrequencyTypes { get; }

        public RelationOptionsDTO(
            ItSystemUsage fromSystemUsage,
            ItSystemUsage toSystemUsage,
            IEnumerable<ItInterface> availableInterfaces,
            IEnumerable<ItContract> availableContracts,
            IEnumerable<RelationFrequencyType> availableFrequencyTypes)
        {
            FromSystemUsage = fromSystemUsage;
            ToSystemUsage = toSystemUsage;
            AvailableInterfaces = availableInterfaces;
            AvailableContracts = availableContracts;
            AvailableFrequencyTypes = availableFrequencyTypes;
        }
    }
}
