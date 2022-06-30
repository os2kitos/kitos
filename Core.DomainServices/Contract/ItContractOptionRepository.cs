using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainServices.Model.Options;
using Core.DomainServices.Options;

namespace Core.DomainServices.Contract
{
    //TODO: Not needed
    public class ItContractOptionRepository: IItContractOptionRepository
    {
        private readonly IOptionsService<ItContract, CriticalityType> _criticalityTypesService;

        public ItContractOptionRepository(IOptionsService<ItContract, CriticalityType> criticalityTypesService)
        {
            _criticalityTypesService = criticalityTypesService;
        }

        public IEnumerable<OptionDescriptor<CriticalityType>> GetAvailableCriticalityOptions(int organizationId)
        {
            return _criticalityTypesService.GetAvailableOptionsDetails(organizationId);
        }
    }
}
