using System;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataProcessorAssignmentService : IDataProcessingRegistrationDataProcessorAssignmentService
    {
        private readonly IOrganizationRepository _organizationRepository;

        public DataProcessingRegistrationDataProcessorAssignmentService(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public IQueryable<Organization> GetApplicableDataProcessors(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var assignedProcessors = registration.DataProcessors.Select(x => x.Id).ToList();
            return _organizationRepository.GetAll().ExceptEntitiesWithIds(assignedProcessors);
        }

        public Result<Organization, OperationError> AssignDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return
                _organizationRepository
                    .GetById(organizationId)
                    .Select(registration.AssignDataProcessor)
                    .Match(result => result, () => new OperationError("Invalid organization id", OperationFailure.BadInput));
        }

        public Result<Organization, OperationError> RemoveDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return
                _organizationRepository
                    .GetById(organizationId)
                    .Select(registration.RemoveDataProcessor)
                    .Match(result => result, () => new OperationError("Invalid organization id", OperationFailure.BadInput));
        }
    }
}
