using Core.DomainModel;

namespace Core.ApplicationServices.Authorization.Policies
{
    public interface IModuleModificationPolicy
    {
        bool AllowModification(IEntity entity);
    }
}
