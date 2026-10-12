using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainServices.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [Authorize]
    public abstract class BaseApiController : ExtendedApiController
    {
        protected IOrganizationalUserContext UserContext => HttpContext.RequestServices.GetRequiredService<IOrganizationalUserContext>();
        protected IAuthorizationContext AuthorizationContext => HttpContext.RequestServices.GetRequiredService<IAuthorizationContext>();

        private IControllerAuthorizationStrategy CreateAuthorizationStrategy() =>
            new ContextBasedAuthorizationStrategy(AuthorizationContext);

        protected IControllerAuthorizationStrategy AuthorizationStrategy => CreateAuthorizationStrategy();
        protected IControllerCrudAuthorization CrudAuthorization => GetCrudAuthorization();

        protected BaseApiController()
        {
        }

        protected virtual IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new RootEntityCrudAuthorization(AuthorizationStrategy);
        }

        protected EntityReadAccessLevel GetEntityTypeReadAccessLevel<T>()
        {
            return AuthorizationStrategy.GetEntityTypeReadAccessLevel<T>();
        }

        protected CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccessLevel()
        {
            return AuthorizationStrategy.GetCrossOrganizationReadAccess();
        }

        protected OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return AuthorizationStrategy.GetOrganizationReadAccessLevel(organizationId);
        }

        protected bool AllowRead(IEntity entity) => CrudAuthorization.AllowRead(entity);
        protected bool AllowModify(IEntity entity) => CrudAuthorization.AllowModify(entity);

        protected virtual bool AllowCreate<T>(int organizationId, IEntity entity)
            => CrudAuthorization.AllowCreate<T>(organizationId, entity);

        protected bool AllowCreate<T>(int organizationId)
            => AuthorizationStrategy.AllowCreate<T>(organizationId);

        protected bool AllowDelete(IEntity entity) => CrudAuthorization.AllowDelete(entity);

        protected bool AllowEntityVisibilityControl(IEntity entity)
            => AuthorizationStrategy.HasPermission(new VisibilityControlPermission(entity));

        protected virtual IEntity GetEntity(int id) =>
            throw new NotSupportedException("This endpoint does not support access rights");

        protected virtual bool AllowCreateNewEntity(int organizationId) =>
            throw new NotSupportedException("This endpoint does not support generic creation rights");

        [HttpGet]
        [InternalApi]
        public virtual IActionResult GetAccessRights(bool? getEntitiesAccessRights, int organizationId)
        {
            if (GetOrganizationReadAccessLevel(organizationId) == OrganizationDataReadAccessLevel.None)
                return Forbidden();

            return Ok(new EntitiesAccessRightsDTO
            {
                CanCreate = AllowCreateNewEntity(organizationId),
                CanView = true
            });
        }

        [HttpGet]
        [InternalApi]
        public virtual IActionResult GetAccessRightsForEntity(int id, bool? getEntityAccessRights)
        {
            var item = GetEntity(id);
            if (item == null)
                return NotFound();

            return Ok(GetAccessRightsForEntityItem(item));
        }

        private EntityAccessRightsDTO GetAccessRightsForEntityItem(IEntity item)
        {
            return new EntityAccessRightsDTO
            {
                Id = item.Id,
                CanDelete = AllowDelete(item),
                CanEdit = AllowModify(item),
                CanView = AllowRead(item)
            };
        }

        [HttpPost]
        [InternalApi]
        public virtual IActionResult PostSearchAccessRightsForEntityList([FromBody] int[] ids, bool? getEntityListAccessRights)
        {
            if (ids == null || ids.Length == 0)
                return BadRequest();

            return Ok(
                ids
                    .Distinct()
                    .Select(GetEntity)
                    .Where(entity => entity != null)
                    .Select(GetAccessRightsForEntityItem)
                    .ToList()
            );
        }
    }
}
