
using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GDPR;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.GDPR;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationOptionsApplicationService : IDataProcessingRegistrationOptionsApplicationService
    {
        private readonly IDataProcessingRegistrationOptionRepository _optionRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public DataProcessingRegistrationOptionsApplicationService(
            IAuthorizationContext authorizationContext,
            IDataProcessingRegistrationOptionRepository optionRepository)
        {
            _authorizationContext = authorizationContext;
            _optionRepository = optionRepository;
        }

        public Result<DataProcessingRegistrationOptions, OperationError> GetAssignableDataProcessingRegistrationOptions(int organizationId)
        {
            return WithOrganizationReadAccess(organizationId,
                () => new DataProcessingRegistrationOptions(
                        _optionRepository.GetAvailableDataResponsibleOptions(organizationId),
                        _optionRepository.GetAvailableCountryOptions(organizationId),
                        _optionRepository.GetAvailableBasisForTransferOptions(organizationId),
                        _optionRepository.GetAvailableRoles(organizationId),
                        _optionRepository.GetAvailableOversightOptions(organizationId)
                    ));
        }

        private Result<DataProcessingRegistrationOptions, OperationError> WithOrganizationReadAccess(int organizationId, Func<Result<DataProcessingRegistrationOptions, OperationError>> authorizedAction)
        {
            var readAccessLevel = _authorizationContext.GetOrganizationReadAccessLevel(organizationId);

            return readAccessLevel < OrganizationDataReadAccessLevel.All
                ? new OperationError(OperationFailure.Forbidden)
                : authorizedAction();
        }
    }
}
