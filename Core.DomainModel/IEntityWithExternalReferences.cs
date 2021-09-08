using Core.Abstractions.Types;
using Core.DomainModel.References;


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
