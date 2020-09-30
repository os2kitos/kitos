
using System.Collections.Generic;
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

        public IEnumerable<DataProcessingCountryOption> GetAvailableCountryOptions(int organizationId)
        {
            return _countryOptionsService.GetAvailableOptions(organizationId);
        }

        public IEnumerable<DataProcessingDataResponsibleOption> GetAvailableDataResponsibleOptions(int organizationId)
        {
            return _dataResponsibleOptionsService.GetAvailableOptions(organizationId);
        }
    }
}
