using Core.DomainModel.References;

namespace Core.DomainModel
{
    public interface IEntityWithExternalReferences : IEntity, IHasReferences
    {
        ReferenceRootType GetRootType();
    }
}
