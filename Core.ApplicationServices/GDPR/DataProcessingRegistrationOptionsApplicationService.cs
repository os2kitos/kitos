
using System;
using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GDPR;
using Core.DomainModel.Result;
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
                        _optionRepository.GetAvailableDataResponsibleOptionsWithLocallyUpdatedDescriptions(organizationId),
                        _optionRepository.GetAvailableCountryOptions(organizationId)
                    ));
        }

        public ISet<int> GetIdsOfAvailableCountryOptions(int organizationId)
        {
            return _optionRepository.GetIdsOfAvailableCountryOptions(organizationId);
        }

        public ISet<int> GetIdsOfAvailableDataResponsibleOptions(int organizationId)
        {
            return _optionRepository.GetIdsOfAvailableDataResponsibleOptions(organizationId);
        }

        private Result<DataProcessingRegistrationOptions, OperationError> WithOrganizationReadAccess(
            int organizationId, 
            Func<Result<DataProcessingRegistrationOptions, OperationError>> authorizedAction)
        {
            var readAccessLevel = _authorizationContext.GetOrganizationReadAccessLevel(organizationId);
            if (readAccessLevel > OrganizationDataReadAccessLevel.None)
                return authorizedAction();

            return new OperationError(OperationFailure.Forbidden);
        }
    }
}
