using Core.DomainModel.References;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainModel
{
    public interface IEntityWithExternalReferences : IEntity, IHasReferences
    {
        ReferenceRootType GetRootType();
        Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference);
        Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference);
        void ClearMasterReference();
    }
}
