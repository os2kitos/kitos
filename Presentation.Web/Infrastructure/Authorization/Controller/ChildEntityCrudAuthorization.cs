using System;
using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public class ChildEntityCrudAuthorization<T> : IControllerCrudAuthorization
    {
        private readonly Func<T, IEntity> _getRoot;
        private readonly IControllerCrudAuthorization _crudAuthorization;

        public ChildEntityCrudAuthorization(Func<T, IEntity> getRoot,
            IControllerCrudAuthorization crudAuthorization)
        {
            _getRoot = getRoot;
            _crudAuthorization = crudAuthorization;
        }

        public bool AllowRead(IEntity entity)
        {
            return DelegateToRootEntity(entity, _crudAuthorization.AllowRead);
        }

        public bool AllowCreate<T1>(IEntity entity)
        {
            return
                typeof(T1) == typeof(T) &&
                AllowModify(entity); //Even though it is AllowCreate, we delegate to AllowModify on the root
        }

        public bool AllowModify(IEntity entity)
        {
            return DelegateToRootEntity(entity, _crudAuthorization.AllowModify);
        }

        public bool AllowDelete(IEntity entity)
        {
            //Even though it is AllowDelete, we delegate to AllowModify on the root
            return AllowModify(entity);
        }

        private bool DelegateToRootEntity(IEntity entity, Predicate<IEntity> authorize)
        {
            if (entity is T inputType)
            {
                var root = _getRoot(inputType);
                if (root != null)
                {
                    return authorize.Invoke(root);
                }
            }

            return false;
        }
    }
}