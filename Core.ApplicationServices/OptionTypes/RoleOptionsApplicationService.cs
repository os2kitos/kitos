using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.OptionTypes
{
    public class RoleOptionsApplicationService<TReference, TOption> : BaseOptionsApplicationService<TReference, TOption>, IRoleOptionsApplicationService<TReference, TOption>
        where TOption : OptionEntity<TReference>
    {
        private readonly IRoleOptionsService<TReference, TOption> _roleOptionsApplicationService;

        public RoleOptionsApplicationService(IAuthorizationContext authorizationContext, IOrganizationRepository organizationRepository, IRoleOptionsService<TReference, TOption> roleOptionsApplicationService) : base(organizationRepository, authorizationContext)
        {
            _roleOptionsApplicationService = roleOptionsApplicationService;
        }

        public Result<IEnumerable<OptionDescriptor<TOption>>, OperationError> GetOptionTypes(Guid organizationUuid)
        {
            return ResolveOrgId(organizationUuid)
                .Select(_roleOptionsApplicationService.GetAvailableOptionsDetails)
                .Select(SortOptionsByPriority);
        }
    }
}
