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
                ? config.Value
                : Result<KendoOrganizationalConfiguration, OperationError>.Failure(OperationFailure.NotFound);
        }

        public Result<KendoOrganizationalConfiguration, OperationError> CreateOrUpdate(int organizationId, OverviewType overviewType, IEnumerable<KendoColumnConfiguration> columns)
        {
            var currentConfig = _kendoOrganizationRepository.Get(organizationId, overviewType);

            if (currentConfig.HasValue)
            {
                var modifiedConfig = currentConfig.Value;
                if (!_authorizationContext.AllowModify(modifiedConfig))
                    return new OperationError(OperationFailure.Forbidden);

                modifiedConfig.Columns.Clear();
                _kendoOrganizationRepository.Update(modifiedConfig); //Clean-out the old entries

                columns.ToList().ForEach(x => 
                {
                    x.KendoOrganizationalConfigurationId = modifiedConfig.Id;
                    modifiedConfig.Columns.Add(x);
                });

                modifiedConfig.UpdateVersion();
                _kendoOrganizationRepository.Update(modifiedConfig);
                return modifiedConfig;
            }

            if (!_authorizationContext.AllowCreate<KendoOrganizationalConfiguration>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            var createdConfig = new KendoOrganizationalConfiguration
            {
                OrganizationId = organizationId,
                OverviewType = overviewType
            };
            createdConfig.UpdateVersion(); // Version is required on the entity
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
