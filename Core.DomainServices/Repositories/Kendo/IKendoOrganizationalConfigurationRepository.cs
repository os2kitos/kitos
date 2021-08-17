using System.Linq;
using Core.DomainModel;
using Core.DomainModel.KendoConfig;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Kendo
{
    public interface IKendoOrganizationalConfigurationRepository
    {
        public Maybe<KendoOrganizationalConfiguration> Get(int organizationId, OverviewType overviewType);
        public void Update(KendoOrganizationalConfiguration modifiedConfig);
        public KendoOrganizationalConfiguration Add(KendoOrganizationalConfiguration createdConfig);
        void Delete(KendoOrganizationalConfiguration configToDelete);
        void DeleteColumns(KendoOrganizationalConfiguration configWithColumnsToBeDeleted);
    }
}
