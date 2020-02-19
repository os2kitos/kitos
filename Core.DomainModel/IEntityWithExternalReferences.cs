using Core.DomainModel.References;
using Core.DomainModel.Result;

namespace Core.DomainModel
{
    public interface IEntityWithExternalReferences : IEntity, IHasReferences
    {
        ReferenceRootType GetRootType();
        Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference);
        Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference);
    }
}
