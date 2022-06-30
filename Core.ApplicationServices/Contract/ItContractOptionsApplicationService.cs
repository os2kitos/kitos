using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Contracts;
using Core.DomainServices.Authorization;
using Core.DomainServices.Contract;

namespace Core.ApplicationServices.Contract
{
    public class ItContractOptionsApplicationService : IItContractOptionsApplicationService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItContractOptionRepository _optionRepository;

        public ItContractOptionsApplicationService(IAuthorizationContext authorizationContext, IItContractOptionRepository optionRepository)
        {
            _authorizationContext = authorizationContext;
            _optionRepository = optionRepository;
        }
        
        public Result<ContractOptions, OperationError> GetAssignableContractOptions(int organizationId)
        {
            return WithOrganizationReadAccess(organizationId,
                () => new ContractOptions(
                    _optionRepository.GetAvailableCriticalityOptions(organizationId)));
        }

        private Result<ContractOptions, OperationError> WithOrganizationReadAccess(int organizationId, Func<Result<ContractOptions, OperationError>> authorizedAction)
        {
            var readAccessLevel = _authorizationContext.GetOrganizationReadAccessLevel(organizationId);

            return readAccessLevel < OrganizationDataReadAccessLevel.All
                ? new OperationError(OperationFailure.Forbidden)
                : authorizedAction();
        }
    }
}
