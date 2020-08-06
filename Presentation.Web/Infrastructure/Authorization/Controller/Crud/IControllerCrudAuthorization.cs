using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller.Crud
{
    public interface IControllerCrudAuthorization
    {
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(int organizationId, IEntity entity);
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
    }
}
