using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainServices.Model.Options;
using Core.DomainServices.Options;

namespace Core.DomainServices.Repositories.GDPR
{
    public class DataProcessingRegistrationOptionRepository : IDataProcessingRegistrationOptionRepository
    {
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> _countryOptionsService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> _dataResponsibleOptionsService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> _basisForTransferOptionsService;
        private readonly IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole> _roleOptionsService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingOversightOption> _oversightOptionsService;

        public DataProcessingRegistrationOptionRepository(
            IOptionsService<DataProcessingRegistration, DataProcessingCountryOption> countryOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> dataResponsibleOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> basisForTransferOptionsService,
            IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole> roleOptionsService,
            IOptionsService<DataProcessingRegistration, DataProcessingOversightOption> oversightOptionsService)
        {
            _countryOptionsService = countryOptionsService;
            _dataResponsibleOptionsService = dataResponsibleOptionsService;
            _basisForTransferOptionsService = basisForTransferOptionsService;
            _roleOptionsService = roleOptionsService;
            _oversightOptionsService = oversightOptionsService;
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

        public IEnumerable<OptionDescriptor<DataProcessingRegistrationRole>> GetAvailableRoles(int organizationId)
        {
            return _roleOptionsService.GetAvailableOptionsDetails(organizationId);
        }
        public IEnumerable<OptionDescriptor<DataProcessingOversightOption>> GetAvailableOversightOptions(int organizationId)
        {
            return _oversightOptionsService.GetAvailableOptionsDetails(organizationId);
        }
    }
}
