using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.LocalOption
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
            var organization = _organizationRepository.GetByUuid(organizationUuid);
            if (organization.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }
            var orgId = organization.Value.Id;

            if (_authorizationContext.GetOrganizationReadAccessLevel(orgId) < DomainServices.Authorization.OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            if(_businessTypeOptionService.GetOptionByUuid(orgId, businessTypeUuid).IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }

            return Result<(BusinessType option, bool available), OperationError>.Success(_businessTypeOptionService.GetOptionByUuid(orgId, businessTypeUuid).Value);
        }

        public Result<IEnumerable<BusinessType>, OperationError> GetBusinessTypes(Guid organizationUuid)
        {
            var organization = _organizationRepository.GetByUuid(organizationUuid);
            if (organization.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }
            var orgId = organization.Value.Id;

            if (_authorizationContext.GetOrganizationReadAccessLevel(orgId) < DomainServices.Authorization.OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return Result<IEnumerable<BusinessType>, OperationError>.Success(_businessTypeOptionService.GetAvailableOptions(orgId));
        }
    }
}
