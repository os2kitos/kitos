using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices.Model.Options;

namespace Core.ApplicationServices.Model.Contracts
{
    public class ContractOptions
    {
        public IReadOnlyList<OptionDescriptor<CriticalityType>> CriticalityOptions { get; }

        public ContractOptions(
            IEnumerable<OptionDescriptor<CriticalityType>> dataResponsibleOptions)
        {
            CriticalityOptions = dataResponsibleOptions.ToList();
        }
    }
}
