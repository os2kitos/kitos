using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.OptionTypes
{
    public class BusinessTypeApplicationService : IBusinessTypeApplicationService
    {
        private readonly IOptionsService<ItSystem, BusinessType> _businessTypeOptionService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public BusinessTypeApplicationService(IOptionsService<ItSystem, BusinessType> businessTypeOptionService, IAuthorizationContext authorizationContext, IOrganizationRepository organizationRepository)
        {
            _businessTypeOptionService = businessTypeOptionService;
            _authorizationContext = authorizationContext;
            _organizationRepository = organizationRepository;
        }

        public Result<(BusinessType option, bool available), OperationError> GetBusinessType(Guid organizationUuid, Guid businessTypeUuid)
        {
            var orgId = ResolveOrgIdAndAccessLevel(organizationUuid);
            if (orgId.Failed)
            {
                return orgId.Error;
            }
            var businessTypeResult = _businessTypeOptionService.GetOptionByUuid(orgId.Value, businessTypeUuid);
            if (businessTypeResult.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            return businessTypeResult.Value;
        }

        public Result<IEnumerable<BusinessType>, OperationError> GetBusinessTypes(Guid organizationUuid)
        {
            var orgId = ResolveOrgIdAndAccessLevel(organizationUuid);
            if (orgId.Failed)
            {
                return orgId.Error;
            }

            return Result<IEnumerable<BusinessType>, OperationError>.Success(_businessTypeOptionService.GetAvailableOptions(orgId.Value));
        }

        private Result<int, OperationError> ResolveOrgIdAndAccessLevel(Guid organizationUuid)
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
