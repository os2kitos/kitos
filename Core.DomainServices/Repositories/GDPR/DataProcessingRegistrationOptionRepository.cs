
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainModel.LocalOptions;
using Core.DomainServices.Options;

namespace Core.DomainServices.Repositories.GDPR
{
    public class DataProcessingRegistrationOptionRepository : IDataProcessingRegistrationOptionRepository
    {
        private readonly IGenericRepository<LocalDataProcessingDataResponsibleOption> _localDataResponsibleOptionRepository;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> _countryOptionsService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> _dataResponsibleOptionsService;

        public DataProcessingRegistrationOptionRepository(
            IGenericRepository<LocalDataProcessingDataResponsibleOption> localdataResponsibleOptionRepository,
            IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> countryOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> dataResponsibleOptionsService)
        {
            _localDataResponsibleOptionRepository = localdataResponsibleOptionRepository;
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
