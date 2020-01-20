using System;
using Core.DomainModel;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model.Result;

namespace Presentation.Web.Infrastructure.Authorization.Controller.Crud
{
    public class ChildEntityCrudAuthorization<T, TRoot> : IControllerCrudAuthorization
        where TRoot : class, IEntity
    {
        private readonly Func<T, TRoot> _getRoot;
        private readonly IControllerCrudAuthorization _crudAuthorization;

        public ChildEntityCrudAuthorization(Func<T, TRoot> getRoot,
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
            return AllowModify(entity);
        }

        public bool AllowModify(IEntity entity)
        {
            return DelegateToRootEntity(entity, _crudAuthorization.AllowModify);
        }

        public bool AllowDelete(IEntity entity)
        {
            return AllowModify(entity);
        }

        private bool DelegateToRootEntity(IEntity entity, Func<IEntity, bool> authorize)
        {
            var root = Maybe<IEntity>.None;

            switch (entity)
            {
                case TRoot _:
                    root = Maybe<IEntity>.Some(entity);
                    break;
                case T inputType:
                    root = _getRoot(inputType).FromNullable<IEntity>();
                    break;
            }

            return root
                .Select(authorize)
                .GetValueOrFallback(false);
        }
    }
}