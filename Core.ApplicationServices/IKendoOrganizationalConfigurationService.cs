using Core.DomainModel;
using Core.DomainModel.Result;

namespace Core.ApplicationServices
{
    public interface IKendoOrganizationalConfigurationService
    {
        public Result<KendoOrganizationalConfiguration, OperationError> CreateOrUpdate(int organizationId, OverviewType overviewType, string configuration);
        public Result<KendoOrganizationalConfiguration, OperationError> Get(int organizationId, OverviewType overviewType);
    }
}
