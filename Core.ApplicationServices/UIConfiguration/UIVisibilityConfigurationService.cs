using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.UIConfiguration;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.UIConfiguration
{
    public class UIVisibilityConfigurationService : IUIVisibilityConfigurationService
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IUIVisibilityConfigurationRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;

        public UIVisibilityConfigurationService(ITransactionManager transactionManager, 
            IUIVisibilityConfigurationRepository repository,
            IAuthorizationContext authorizationContext)
        {
            _transactionManager = transactionManager;
            _repository = repository;
            _authorizationContext = authorizationContext;
        }


        public Result<List<UIVisibilityConfiguration>, OperationError> GetModuleConfigurationForOrganization(int organizationId, string module)
        {
            var result = _repository.GetModuleConfigurationForOrganization(organizationId, module).ToList();
            if(result.Count() < 1) 
                return new OperationError(OperationFailure.NotFound);
            //if(_authorizationContext.AllowReads())

            return result;
        }

        public Result<List<UIVisibilityConfiguration>, OperationError> Put(int organizationId, string module, List<UIVisibilityConfiguration> configurations)
        {
            if (IsAnyKeyInvalid(configurations))
                return new OperationError(OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin();

            var configurationEntities = _repository.GetModuleConfigurationForOrganization(organizationId, module).ToList();
            
            foreach (var config in configurations)
            {
                if (!_authorizationContext.AllowModify(config))
                    return new OperationError(OperationFailure.Forbidden);

                config.OrganizationId = organizationId;
                config.Module = module;
            }

            configurations.MirrorTo(configurationEntities, prp => prp.Key);

            _repository.UpdateRange(configurationEntities);
            transaction.Commit();

            return configurationEntities;
        }

        private bool IsAnyKeyInvalid(List<UIVisibilityConfiguration> configurations)
        {
            //TODO: Get regex from constants class
            var searchExpresion = new Regex("^([a-zA-Z]+)(\\.[a-zA-Z]+)*$");

            if (configurations.Any(x => searchExpresion.Matches(x.Key).Count < 1))
                return false;

            return true;
        }
    }
}
