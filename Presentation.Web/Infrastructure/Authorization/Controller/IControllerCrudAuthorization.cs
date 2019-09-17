using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public interface IControllerCrudAuthorization
    {
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(IEntity entity);
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
    }
}
