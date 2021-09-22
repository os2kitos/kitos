﻿using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationDataProcessorAssignmentService
    {
        IQueryable<Organization> GetApplicableDataProcessors(DataProcessingRegistration registration);
        Result<Organization, OperationError> AssignDataProcessor(DataProcessingRegistration registration, int organizationId);
        Result<Organization, OperationError> RemoveDataProcessor(DataProcessingRegistration registration, int organizationId);
        IQueryable<Organization> GetApplicableSubDataProcessors(DataProcessingRegistration registration);
        Result<Organization, OperationError> AssignSubDataProcessor(DataProcessingRegistration registration, int organizationId);
        Result<Organization, OperationError> RemoveSubDataProcessor(DataProcessingRegistration registration, int organizationId);
    }
}
