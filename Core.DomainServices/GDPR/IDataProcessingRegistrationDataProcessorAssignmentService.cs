using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationDataProcessorAssignmentService
    {
        IQueryable<Organization> GetApplicableDataProcessors(DataProcessingRegistration registration);
        Result<Organization, OperationError> AssignDataProcessor(DataProcessingRegistration registration, int organizationId);
        Result<Organization, OperationError> RemoveDataProcessor(DataProcessingRegistration registration, int organizationId);
        IQueryable<Organization> GetApplicableSubDataProcessors(DataProcessingRegistration registration);
        Result<SubDataProcessor, OperationError> AssignSubDataProcessor(DataProcessingRegistration registration, int organizationId);
        Result<SubDataProcessor, OperationError> RemoveSubDataProcessor(DataProcessingRegistration registration, int organizationId);
        Result<SubDataProcessor, OperationError> UpdateSubDataProcessor(DataProcessingRegistration registration, int organizationId, int? basisForTransferOptionId, YesNoUndecidedOption? transfer, int? insecureCountryOptionId);
    }
}
