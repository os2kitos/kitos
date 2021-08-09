using Core.ApplicationServices.Organizations;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using System;
using System.Linq;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationUuidExtensionService : IDataProcessingRegistrationUuidExtensionService
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IOrganizationService _organizationService;

        public DataProcessingRegistrationUuidExtensionService(IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService, IOrganizationService organizationService)
        {
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
            _organizationService = organizationService;
        }

        public Result<IQueryable<DataProcessingRegistration>, OperationError> GetDataProcessingRegistrationsByOrganization(Guid orgUuid, params IDomainQuery<DataProcessingRegistration>[] conditions)
        {
            var orgResult = _organizationService.GetOrganization(orgUuid);
            if (orgResult.Failed)
            {
                return orgResult.Error;
            }

            return Result<IQueryable<DataProcessingRegistration>, OperationError>.Success(_dataProcessingRegistrationApplicationService.GetDataProcessingRegistrationsByOrganization(orgResult.Value.Id, conditions));
        }
    }
}
