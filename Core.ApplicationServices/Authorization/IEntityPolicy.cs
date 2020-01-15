using Core.DomainModel;

namespace Core.ApplicationServices.Authorization
{
    public interface IEntityPolicy
    {
        bool Allow(IEntity target);
    }
}
