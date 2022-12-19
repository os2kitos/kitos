using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;

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
            return GetApplicable(registration, _ => _.DataProcessors);
        }

        public Result<Organization, OperationError> AssignDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<Organization>(registration, organizationId, _ => _.AssignDataProcessor);
        }

        public Result<Organization, OperationError> RemoveDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<Organization>(registration, organizationId, _ => _.RemoveDataProcessor);
        }

        public IQueryable<Organization> GetApplicableSubDataProcessors(DataProcessingRegistration registration)
        {
            return GetApplicable(registration, _ => _.SubDataProcessors);
        }

        //TODO: Change the type here!
        public Result<Organization, OperationError> AssignSubDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<Organization>(registration, organizationId, _ => _.AssignSubDataProcessor);
        }

        //TODO: Change the type
        public Result<Organization, OperationError> RemoveSubDataProcessor(DataProcessingRegistration registration, int organizationId)
        {
            return ExecuteDataProcessorChange<Organization>(registration, organizationId, _ => _.RemoveSubDataProcessor);
        }

        //TODO: Add an update method here or inside the domain in stead

        private Result<TResult, OperationError> ExecuteDataProcessorChange<TResult>(
            DataProcessingRegistration registration,
            int organizationId,
            Func<DataProcessingRegistration, Func<Organization, Result<TResult, OperationError>>> getCommand)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return
                _organizationRepository
                    .GetById(organizationId)
                    .Select(getCommand(registration))
                    .Match(result => result, () => new OperationError("Invalid organization id", OperationFailure.BadInput));
        }

        private IQueryable<Organization> GetApplicable(DataProcessingRegistration registration, Func<DataProcessingRegistration, ICollection<Organization>> getTargetCollection)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            var assignedProcessors = getTargetCollection(registration).Select(x => x.Id).ToList();
            return _organizationRepository.GetAll().ExceptEntitiesWithIds(assignedProcessors);
        }
    }
}
