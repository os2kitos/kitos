using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainServices.Authorization;
using Core.DomainServices.Model.Options;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.OptionTypes
{
    public class BaseOptionsApplicationService<TReference, TOption>
        where TOption : OptionEntity<TReference>
    {
        protected readonly IOrganizationRepository _organizationRepository;
        protected readonly IAuthorizationContext _authorizationContext;

        public BaseOptionsApplicationService(IOrganizationRepository organizationRepository, IAuthorizationContext authorizationContext)
        {
            _organizationRepository = organizationRepository;
            _authorizationContext = authorizationContext;
        }

        protected static IEnumerable<OptionDescriptor<TOption>> SortOptionsByPriority(IEnumerable<OptionDescriptor<TOption>> options)
        {
            return options.OrderByDescending(x => x.Option.Priority);
        }

        protected Result<int, OperationError> ResolveOrgId(Guid organizationUuid)
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
