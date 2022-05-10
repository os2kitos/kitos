using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.UIConfiguration;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.UIConfiguration
{
    public class UIModuleCustomizationService : IUIModuleCustomizationService
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IUIModuleCustomizationRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;

        public UIModuleCustomizationService(ITransactionManager transactionManager, 
            IUIModuleCustomizationRepository repository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext)
        {
            _transactionManager = transactionManager;
            _repository = repository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
        }


        public Result<List<UIModuleCustomization>, OperationError> GetModuleConfigurationForOrganization(int organizationId, string module)
        {
            var result = _repository.GetModuleConfigurationForOrganization(organizationId, module).ToList();
            if(result.Count < 1) 
                return new OperationError(OperationFailure.NotFound);
            //if(_authorizationContext.AllowReads())

            return result;
        }

        public Result<List<UIModuleCustomization>, OperationError> Put(int organizationId, string module, UIModuleCustomization configuration)
        {
            /*if (IsAnyKeyInvalid(configurations))
                return new OperationError(OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin();

            //TODO: Allowed to modify the org (which owns the modules¤)
            //TODO: Add the functionality (merging, uniqueness check etc to the org domain model)
            //_userContext.HasRole(organizationId,OrganizationRole.LocalAdmin)

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

            return configurationEntities;*/
            throw new NotImplementedException();
        }

        private bool IsAnyKeyInvalid(List<CustomizedUINode> configurations)
        {
            var searchExpresion = new Regex(UIModuleConfigurationConstants.ConfigurationKeyRegex);

            if (configurations.Any(x => searchExpresion.Matches(x.Key).Count < 1))
                return true;

            return false;
        }
    }
}
