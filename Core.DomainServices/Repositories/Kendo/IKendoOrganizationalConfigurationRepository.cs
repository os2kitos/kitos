using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.KendoConfig;


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
