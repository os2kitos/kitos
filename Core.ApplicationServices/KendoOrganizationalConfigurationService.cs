using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class KendoOrganizationalConfigurationService : IKendoOrganizationalConfigurationService
    {
        private readonly IGenericRepository<KendoOrganizationalConfiguration> _kendoOrganizationRepository;
        public KendoOrganizationalConfigurationService(IGenericRepository<KendoOrganizationalConfiguration> kendoOrganizationRepository)
        {
            _kendoOrganizationRepository = kendoOrganizationRepository;
        }

        public Result<KendoOrganizationalConfiguration, OperationFailure> Add(KendoOrganizationalConfiguration newKendoOrganizationalConfiguration)
        {
            var created = _kendoOrganizationRepository.Insert(newKendoOrganizationalConfiguration);
            _kendoOrganizationRepository.Save();
            return created;
        }

        public Result<KendoOrganizationalConfiguration, OperationFailure> Modify(KendoOrganizationalConfiguration modifiedKendoOrganizationalConfiguration)
        {
            _kendoOrganizationRepository.Update(modifiedKendoOrganizationalConfiguration);
            _kendoOrganizationRepository.Save();
            return modifiedKendoOrganizationalConfiguration;
        }

        public Result<string, OperationFailure> Get(int organizationId, OverviewType overviewType)
        {
            throw new global::System.NotImplementedException();
        }
    }
}
