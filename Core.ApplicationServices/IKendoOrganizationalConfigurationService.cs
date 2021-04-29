using Core.DomainModel;
using Core.DomainModel.Result;

namespace Core.ApplicationServices
{
    public interface IKendoOrganizationalConfigurationService
    {
        public Result<KendoOrganizationalConfiguration, OperationFailure> Add(KendoOrganizationalConfiguration newKendoOrganizationalConfiguration);
        public Result<KendoOrganizationalConfiguration, OperationFailure> Modify(KendoOrganizationalConfiguration newKendoOrganizationalConfiguration);
        public Result<string, OperationFailure> Get(int organizationId, OverviewType overviewType);
    }
}
