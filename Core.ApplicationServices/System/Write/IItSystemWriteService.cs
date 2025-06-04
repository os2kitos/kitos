using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using System;

namespace Core.ApplicationServices.System.Write
{
    public interface IItSystemWriteService
    {
        Result<ItSystem, OperationError> CreateNewSystem(Guid organizationUuid, SystemUpdateParameters parameters);
        Result<ItSystem, OperationError> Update(Guid systemUuid, SystemUpdateParameters parameters);
        Result<ItSystem, OperationError> Delete(Guid systemUuid);

        Result<ItSystem, OperationError> LegalPropertiesUpdate(Guid systemUuid, LegalUpdateParameters parameters);
        Result<ExternalReference, OperationError> AddExternalReference(Guid systemUuid, ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationError> UpdateExternalReference(Guid systemUuid, Guid externalReferenceUuid, ExternalReferenceProperties externalReferenceProperties);
        Result<ExternalReference, OperationError> DeleteExternalReference(Guid systemUuid, Guid externalReferenceUuid);
    }
}
