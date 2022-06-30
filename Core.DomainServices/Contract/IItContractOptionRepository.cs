using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainServices.Model.Options;

namespace Core.DomainServices.Contract
{
    public interface IItContractOptionRepository
    {
        IEnumerable<OptionDescriptor<CriticalityType>> GetAvailableCriticalityOptions(int organizationId);
    }
}
