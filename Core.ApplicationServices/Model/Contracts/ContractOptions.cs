using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices.Model.Options;

namespace Core.ApplicationServices.Model.Contracts
{
    public class ContractOptions
    {
        public IReadOnlyList<(OptionDescriptor<CriticalityType> option, bool available)> CriticalityOptions { get; }

        public ContractOptions(
            IEnumerable<(OptionDescriptor<CriticalityType> option, bool available)> criticalityOptions)
        {
            CriticalityOptions = criticalityOptions.ToList();
        }
    }
}
