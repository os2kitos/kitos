using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public class RootEntityCrudAuthorization : IControllerCrudAuthorization
    {
        private readonly IControllerAuthorizationStrategy _mainStrategy;

        public RootEntityCrudAuthorization(IControllerAuthorizationStrategy mainStrategy)
        {
            _mainStrategy = mainStrategy;
        }

        public bool AllowRead(IEntity entity)
        {
            return _mainStrategy.AllowRead(entity);
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            return _mainStrategy.AllowCreate<T>(entity);
        }

        public bool AllowModify(IEntity entity)
        {
            return _mainStrategy.AllowModify(entity);
        }

        public bool AllowDelete(IEntity entity)
        {
            return _mainStrategy.AllowDelete(entity);
        }
    }
}