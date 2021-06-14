using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.OptionTypes
{
    public class OptionsApplicationService<TReference, TOption> : IOptionsApplicationService<TReference, TOption> 
        where TOption : OptionEntity<TReference>
    {
        private readonly IOptionsService<TReference, TOption> _optionsTypeService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public OptionsApplicationService(IOptionsService<TReference, TOption> optionsTypeService, IAuthorizationContext authorizationContext, IOrganizationRepository organizationRepository)
        {
            _optionsTypeService = optionsTypeService;
            _authorizationContext = authorizationContext;
            _organizationRepository = organizationRepository;
        }

        public Result<(TOption option, bool available), OperationError> GetOptionType(Guid organizationUuid, Guid businessTypeUuid)
        {
            var orgId = ResolveOrgId(organizationUuid);
            if (orgId.Failed)
            {
                return orgId.Error;
            }
            var businessTypeResult = _optionsTypeService.GetOptionByUuid(orgId.Value, businessTypeUuid);
            if (businessTypeResult.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            return businessTypeResult.Value;
        }

        public Result<IEnumerable<TOption>, OperationError> GetOptionTypes(Guid organizationUuid)
        {
            var orgId = ResolveOrgId(organizationUuid);
            if (orgId.Failed)
            {
                return orgId.Error;
            }

            return Result<IEnumerable<TOption>, OperationError>.Success(_optionsTypeService.GetAvailableOptions(orgId.Value));
        }

        private Result<int, OperationError> ResolveOrgId(Guid organizationUuid)
        {
            var organizationId = _organizationRepository.GetByUuid(organizationUuid).Select(org => org.Id);
            if (organizationId.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId.Value) < OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return organizationId.Value;
        }
    }
}
