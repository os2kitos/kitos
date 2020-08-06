using Core.DomainModel;
using Presentation.Web.Infrastructure.Authorization.Controller.General;

namespace Presentation.Web.Infrastructure.Authorization.Controller.Crud
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

        public bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return _mainStrategy.AllowCreate<T>(organizationId, entity);
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