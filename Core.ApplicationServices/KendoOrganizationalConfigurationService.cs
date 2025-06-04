using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Repositories.Kendo;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.KendoConfig;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices
{
    public class KendoOrganizationalConfigurationService : IKendoOrganizationalConfigurationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IKendoOrganizationalConfigurationRepository _kendoOrganizationRepository;
        private readonly ITransactionManager _transactionManager;

        public KendoOrganizationalConfigurationService(
            IAuthorizationContext authorizationContext,
            IKendoOrganizationalConfigurationRepository kendoOrganizationRepository,
            ITransactionManager transactionManager)
        {
            _authorizationContext = authorizationContext;
            _kendoOrganizationRepository = kendoOrganizationRepository;
            _transactionManager = transactionManager;
        }

        public Result<KendoOrganizationalConfiguration, OperationError> Get(int organizationId,
            OverviewType overviewType)
        {
            var config = _kendoOrganizationRepository.Get(organizationId, overviewType);

            if (config.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            return _authorizationContext.AllowReads(config.Value)
                ? config.Value
                : new OperationError(OperationFailure.Forbidden);
        }

        public Result<KendoOrganizationalConfiguration, OperationError> CreateOrUpdate(int organizationId,
            OverviewType overviewType, IEnumerable<KendoColumnConfiguration> columns)
        {
            var transaction = _transactionManager.Begin();
            var currentConfig = _kendoOrganizationRepository.Get(organizationId, overviewType);

            if (currentConfig.HasValue)
            {
                var configToUpdate = currentConfig.Value;
                if (!_authorizationContext.AllowModify(configToUpdate))
                {
                    transaction.Rollback();
                    return new OperationError(OperationFailure.Forbidden);
                }

                var modifiedConfig = UpdateConfig(configToUpdate, columns);
                transaction.Commit();
                return modifiedConfig;
            }

            if (!_authorizationContext.AllowCreate<KendoOrganizationalConfiguration>(organizationId))
            {
                transaction.Rollback();
                return new OperationError(OperationFailure.Forbidden);
            }

            var created = CreateConfig(organizationId, overviewType, columns);
            transaction.Commit();
            return created;
        }

        public Result<KendoOrganizationalConfiguration, OperationError> Delete(int organizationId,
            OverviewType overviewType)
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

        private Result<KendoOrganizationalConfiguration, OperationError> UpdateConfig(
            KendoOrganizationalConfiguration modifiedConfig, IEnumerable<KendoColumnConfiguration> columns)
        {
            _kendoOrganizationRepository.DeleteColumns(modifiedConfig); //Clean-out the old entries
            modifiedConfig.AddColumns(columns);
            _kendoOrganizationRepository.Update(modifiedConfig);
            return modifiedConfig;
        }

        private Result<KendoOrganizationalConfiguration, OperationError> CreateConfig(int organizationId, OverviewType overviewType, IEnumerable<KendoColumnConfiguration> columns)
        {
            var createdConfig = KendoOrganizationalConfiguration.CreateConfiguration(organizationId, overviewType);
            createdConfig.AddColumns(columns);
            var created = _kendoOrganizationRepository.Add(createdConfig);
            return created;
        }
    }
}
