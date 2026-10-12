using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.UiCustomization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Core.DomainServices.Repositories.UICustomization;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.UIConfiguration
{
    public class UIModuleCustomizationService : IUIModuleCustomizationService
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOrganizationService _organizationService;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IUIModuleCustomizationRepository _repository;

        public UIModuleCustomizationService(ITransactionManager transactionManager,
            IOrganizationalUserContext userContext,
            IOrganizationService organizationService,
            IEntityIdentityResolver identityResolver,
            IUIModuleCustomizationRepository repository)
        {
            _transactionManager = transactionManager;
            _userContext = userContext;
            _organizationService = organizationService;
            _identityResolver = identityResolver;
            _repository = repository;
        }


        public Result<UIModuleCustomization, OperationError> GetModuleCustomizationForOrganization(int organizationId, string module)
        {
            if (string.IsNullOrEmpty(module))
                throw new ArgumentNullException("Module parameter is null");

            return GetOrganizationById(organizationId)
                .Select(_ => GetModule(organizationId, module));
        }

        public Result<UIModuleCustomization, OperationError> GetModuleCustomizationByOrganizationUuid(Guid organizationUuid, string module)
        {
            return _identityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match(dbId => GetModuleCustomizationForOrganization(dbId, module),
                    () => new OperationError($"Unable to resolve organization id for organization UUID: {organizationUuid}", OperationFailure.NotFound));
        }

        public Result<UIModuleCustomization, OperationError> UpdateModuleAndGet(UIModuleCustomizationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin();
            var result = GetOrganizationById(parameters.OrganizationId)
                .Bind<UIModuleCustomization>(organization =>
                {
                    if (!_userContext.HasRole(parameters.OrganizationId, OrganizationRole.LocalAdmin))
                        return new OperationError("User is not a local admin in organization", OperationFailure.Forbidden);

                    if (string.IsNullOrEmpty(parameters.Module))
                        throw new ArgumentNullException(nameof(parameters.Module));

                    var module = GetModule(parameters.OrganizationId, parameters.Module);
                    var nodesBefore = module.Nodes.ToList();
                    var error = module.UpdateConfigurationNodes(MapNodeParametersToCustomizedUiNodes(parameters.Nodes));
                    if (error.HasValue)
                        return error.Value;

                    var deletedNodes = nodesBefore.Except(module.Nodes).ToList();
                    if (deletedNodes.Count > 0)
                        _repository.DeleteNodes(deletedNodes);
                    _repository.Update(module);
                    return module;
                });
            if (result.Ok)
                transaction.Commit();
            return result;
        }

        private UIModuleCustomization GetModule(int organizationId, string module)
        {
            return _repository.GetByOrganizationAndModule(organizationId, module)
                .GetValueOrFallback(new UIModuleCustomization { OrganizationId = organizationId, Module = module });
        }

        private Result<Organization, OperationError> GetOrganizationById(int organizationId)
        {
            return _identityResolver
                .ResolveUuid<Organization>(organizationId)
                .Select(uuid => _organizationService.GetOrganization(uuid, OrganizationDataReadAccessLevel.All))
                .Match(
                    result => result,
                    () => Result<Organization, OperationError>.Failure(new OperationError(
                    $"Organization uuid could not be resolved from id:{organizationId}", OperationFailure.NotFound))
                    );
        }

        private static IEnumerable<CustomizedUINode> MapNodeParametersToCustomizedUiNodes(IEnumerable<CustomUINodeParameters> parameters)
        {
            return parameters.Select(x => new CustomizedUINode { Key = x.Key, Enabled = x.Enabled, Recommended = x.Recommended }).ToList();
        }

    }
}
