using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Kendo;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices
{
    public class KendoOrganizationalConfigurationService : IKendoOrganizationalConfigurationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IKendoOrganizationalConfigurationRepository _kendoOrganizationRepository;
        public KendoOrganizationalConfigurationService(
            IAuthorizationContext authorizationContext, 
            IKendoOrganizationalConfigurationRepository kendoOrganizationRepository)
        {
            _authorizationContext = authorizationContext;
            _kendoOrganizationRepository = kendoOrganizationRepository;
        }

        public Result<KendoOrganizationalConfiguration, OperationError> Get(int organizationId, OverviewType overviewType)
        {
            var config = _kendoOrganizationRepository.Get(organizationId, overviewType);
            return config.HasValue
                ? config.Value //TODO: JMO - må alle læse alles config? - næppe :-) Mangler tjek på allow read
                : Result<KendoOrganizationalConfiguration, OperationError>.Failure(OperationFailure.NotFound);
        }

        //TODO: JMO Behøves du den her? Du har jo metoden ovenfor som henter hele objektet ud og derpå ligger der jo versionen. 
        public Result<string, OperationError> GetVersion(int organizationId, OverviewType overviewType)
        {
            var config = _kendoOrganizationRepository.Get(organizationId, overviewType);
            return config.HasValue
                ? config.Value.Version 
                : Result<string, OperationError>.Failure(OperationFailure.NotFound);
        }

        public Result<KendoOrganizationalConfiguration, OperationError> CreateOrUpdate(int organizationId, OverviewType overviewType, IEnumerable<KendoColumnConfiguration> columns)
        {
            //TODO: JMO - der mangler en overliggende transaktion her
            var currentConfig = _kendoOrganizationRepository.Get(organizationId, overviewType);

            if (currentConfig.HasValue)
            {
                //TODO: JMO - extract til Update method
                var modifiedConfig = currentConfig.Value;
                if (!_authorizationContext.AllowModify(modifiedConfig))
                    return new OperationError(OperationFailure.Forbidden);
                
                //TODO: JMO: DeleteChilds? Det er vel DeleteColumns?
                _kendoOrganizationRepository.DeleteChilds(modifiedConfig); //Clean-out the old entries

                columns.ToList().ForEach(x => 
                {
                    x.KendoOrganizationalConfigurationId = modifiedConfig.Id; //TODO: JMO det her trick skal bare ind i macen af konfig "Update(columns)" som også opdaterer version bagefter
                    modifiedConfig.Columns.Add(x);
                });

                modifiedConfig.UpdateVersion();
                _kendoOrganizationRepository.Update(modifiedConfig);
                return modifiedConfig;
            }

            if (!_authorizationContext.AllowCreate<KendoOrganizationalConfiguration>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            //TODO: JMO extract to Create method
            //TODO: JMO Lav evt en factory method som kræver argumenterne og så sætter org id og overview type
            var createdConfig = new KendoOrganizationalConfiguration
            {
                OrganizationId = organizationId,
                OverviewType = overviewType
            };
            createdConfig.UpdateVersion(); // Version is required on the entity //TODO: JMO - den kan vel bare være tom til at starte med via din factory method som så kalder updateversion inden den returnnerer
            var created = _kendoOrganizationRepository.Add(createdConfig);

            columns.ToList().ForEach(x =>
            {
                x.KendoOrganizationalConfigurationId = created.Id;
                created.Columns.Add(x);
            });
            created.UpdateVersion();
            _kendoOrganizationRepository.Update(created);

            return created;
        }

        public Result<KendoOrganizationalConfiguration, OperationError> Delete(int organizationId, OverviewType overviewType)
        {
            var currentConfig = _kendoOrganizationRepository.Get(organizationId, overviewType);

            if (currentConfig.HasValue)
            {
                var configToBeDeleted = currentConfig.Value;
                if (!_authorizationContext.AllowDelete(configToBeDeleted))
                    return new OperationError(OperationFailure.Forbidden);

                _kendoOrganizationRepository.Delete(configToBeDeleted);
                return configToBeDeleted;
            }

            return new OperationError(OperationFailure.NotFound);
        }
    }
}
