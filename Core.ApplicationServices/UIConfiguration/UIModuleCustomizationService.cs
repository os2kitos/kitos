using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.Repositories.Organization;
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
        private readonly IOrganizationService _organizationService;

        public UIModuleCustomizationService(ITransactionManager transactionManager, 
            IUIModuleCustomizationRepository repository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext,
            IOrganizationService organizationService)
        {
            _transactionManager = transactionManager;
            _repository = repository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
            _organizationService = organizationService;
        }


        public Result<List<UIModuleCustomization>, OperationError> GetModuleConfigurationForOrganization(int organizationId, string module)
        {
            var organization = GetOrganizationById(organizationId);
            if (organization == null)
                return new OperationError(OperationFailure.NotFound);
            if (!_authorizationContext.AllowReads(organization))
                return new OperationError(OperationFailure.Forbidden);

            var uiModules = organization.UIModuleCustomizations.Where(x => string.Equals(x.Module, module)).ToList();
            if(uiModules.Count < 1) 
                return new OperationError(OperationFailure.NotFound);

            return uiModules;
        }

        public Result<UIModuleCustomization, OperationError> Put(int organizationId, string module, UIModuleCustomization configuration)
        {
            var organization = GetOrganizationById(organizationId);
            if (organization == null)
                return new OperationError(OperationFailure.NotFound);
            /*if (!_userContext.HasRole(organizationId, OrganizationRole.LocalAdmin))
                return new OperationError(OperationFailure.Forbidden);*/

            organization.ModifyModuleCustomization(module, configuration.Nodes);

            //TODO: Allowed to modify the org (which owns the modules¤)
            //TODO: Add the functionality (merging, uniqueness check etc to the org domain model)
            
            using var transaction = _transactionManager.Begin();
            _repository.Update(organization);
            transaction.Commit();
            
            return configuration;
        }

        private Organization GetOrganizationById(int organizationId)
        {
            return _organizationService.SearchAccessibleOrganizations().FirstOrDefault(prp => prp.Id == organizationId);
        }
    }
}
