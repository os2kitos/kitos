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
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> _basisForTransferOptionsService;

        public DataProcessingRegistrationOptionRepository(
            IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> countryOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> dataResponsibleOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> basisForTransferOptionsService)
        {
            _countryOptionsService = countryOptionsService;
            _dataResponsibleOptionsService = dataResponsibleOptionsService;
            _basisForTransferOptionsService = basisForTransferOptionsService;
        }

        public IEnumerable<OptionDescriptor<DataProcessingCountryOption>> GetAvailableCountryOptions(int organizationId)
        {
            return _countryOptionsService.GetAvailableOptionsDetails(organizationId);
        }

        public IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> GetAvailableDataResponsibleOptions(int organizationId)
        {
            return _dataResponsibleOptionsService.GetAvailableOptionsDetails(organizationId);
        }
        public IEnumerable<OptionDescriptor<DataProcessingBasisForTransferOption>> GetAvailableBasisForTransferOptions(int organizationId)
        {
            return _basisForTransferOptionsService.GetAvailableOptionsDetails(organizationId);
        }
    }
}
