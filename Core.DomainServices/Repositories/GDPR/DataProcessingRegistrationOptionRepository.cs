using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.DomainServices.Repositories.GDPR
{
    public class DataProcessingRegistrationOptionRepository : IDataProcessingRegistrationOptionRepository
    {
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> _countryOptionsService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> _dataResponsibleOptionsService;

        public DataProcessingRegistrationOptionRepository(
            IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> countryOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> dataResponsibleOptionsService)
        {
            _countryOptionsService = countryOptionsService;
            _dataResponsibleOptionsService = dataResponsibleOptionsService;
        }

        public IEnumerable<OptionDescriptor<DataProcessingCountryOption>> GetAvailableCountryOptions(int organizationId)
        {
            return _countryOptionsService.GetAvailableOptionsDetails(organizationId);
        }

        public IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> GetAvailableDataResponsibleOptions(int organizationId)
        {
            return _dataResponsibleOptionsService.GetAvailableOptionsDetails(organizationId);
        }

        public ISet<int> GetIdsOfAvailableCountryOptions(int organizationId)
        {
            return new HashSet<int>(_countryOptionsService.GetAvailableOptions(organizationId).Select(x => x.Id));
        }

        public ISet<int> GetIdsOfAvailableDataResponsibleOptions(int organizationId)
        {
            return new HashSet<int>(_dataResponsibleOptionsService.GetAvailableOptions(organizationId).Select(x => x.Id));
        }
    }
}
