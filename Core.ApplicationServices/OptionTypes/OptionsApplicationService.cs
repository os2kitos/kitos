using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Authorization;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainServices.Model.Options;

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

        public Result<(OptionDescriptor<TOption> option, bool available), OperationError> GetOptionType(Guid organizationUuid, Guid optionTypeUuid)
        {
            var orgId = ResolveOrgId(organizationUuid);
            if (orgId.Failed)
            {
                return orgId.Error;
            }

            var businessTypeResult = _optionsTypeService
                .GetAllOptionsDetails(orgId.Value)
                .FirstOrDefault(x => x.option.Option.Uuid == optionTypeUuid)
                .FromNullable();

            if (businessTypeResult.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            return businessTypeResult.Value;
        }

        public Result<IEnumerable<OptionDescriptor<TOption>>, OperationError> GetOptionTypes(Guid organizationUuid)
        {
            return ResolveOrgId(organizationUuid)
                .Select(orgId=> _optionsTypeService.GetAvailableOptionsDetails(orgId));
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
