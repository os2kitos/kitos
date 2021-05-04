using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Kendo;

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
            if (config.HasValue)
            { 
                return config.Value;
            }
            return Result<KendoOrganizationalConfiguration, OperationError>.Failure(OperationFailure.NotFound);
        }

        public Result<KendoOrganizationalConfiguration, OperationError> CreateOrUpdate(int organizationId, OverviewType overviewType, string configuration)
        {
            var currentConfig = _kendoOrganizationRepository.Get(organizationId, overviewType);

            if (currentConfig.HasValue)
            {
                var modifiedConfig = currentConfig.Value;
                if (!_authorizationContext.AllowModify(modifiedConfig))
                    return new OperationError(OperationFailure.Forbidden);

                modifiedConfig.Configuration = configuration;
                _kendoOrganizationRepository.Update(modifiedConfig);
                return modifiedConfig;
            }
            else
            {
                if (!_authorizationContext.AllowCreate<KendoOrganizationalConfiguration>(organizationId))
                    return new OperationError(OperationFailure.Forbidden);

                var createdConfig = new KendoOrganizationalConfiguration
                {
                    OrganizationId = organizationId,
                    OverviewType = overviewType,
                    Configuration = configuration
                };
                var created = _kendoOrganizationRepository.Add(createdConfig);
                return created;
            }
        }
    }
}
