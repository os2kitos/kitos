using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage
{
    public class RelationOptionsDTO
    {
        public ItSystemUsage Source { get; }
        public ItSystemUsage Target { get; }
        public IEnumerable<ItInterface> AvailableInterfaces { get; }
        public IEnumerable<ItContract> AvailableContracts { get; }
        public IEnumerable<RelationFrequencyType> AvailableFrequencyTypes { get; }

        public RelationOptionsDTO(
            ItSystemUsage source,
            ItSystemUsage target,
            IEnumerable<ItInterface> availableInterfaces,
            IEnumerable<ItContract> availableContracts,
            IEnumerable<RelationFrequencyType> availableFrequencyTypes)
        {
            Source = source;
            Target = target;
            AvailableInterfaces = availableInterfaces;
            AvailableContracts = availableContracts;
            AvailableFrequencyTypes = availableFrequencyTypes;
        }
    }
}
