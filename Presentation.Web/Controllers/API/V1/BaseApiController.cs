using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainServices.Authorization;
using Ninject;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [Authorize]
    public abstract class BaseApiController : ExtendedApiController
    {
        [Inject]
        public IOrganizationalUserContext UserContext { get; set; }

        [Inject]
        public IAuthorizationContext AuthorizationContext { get; set; }

        //Lazy to make sure auth service is available when resolved
        private readonly Lazy<IControllerAuthorizationStrategy> _authorizationStrategy;
        private readonly Lazy<IControllerCrudAuthorization> _crudAuthorization;

        protected IControllerAuthorizationStrategy AuthorizationStrategy => _authorizationStrategy.Value;

        protected IControllerCrudAuthorization CrudAuthorization => _crudAuthorization.Value;

        protected BaseApiController()
        {
            _authorizationStrategy = new Lazy<IControllerAuthorizationStrategy>(() => new ContextBasedAuthorizationStrategy(AuthorizationContext));
            _crudAuthorization = new Lazy<IControllerCrudAuthorization>(GetCrudAuthorization);
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

        protected bool AllowRead(IEntity entity)
        {
            return CrudAuthorization.AllowRead(entity);
        }

        protected bool AllowModify(IEntity entity)
        {
            return CrudAuthorization.AllowModify(entity);
        }

        protected virtual bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return CrudAuthorization.AllowCreate<T>(organizationId, entity);
        }

        protected bool AllowCreate<T>(int organizationId)
        {
            return AuthorizationStrategy.AllowCreate<T>(organizationId);
        }

        protected bool AllowDelete(IEntity entity)
        {
            return CrudAuthorization.AllowDelete(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return AuthorizationStrategy.HasPermission(new VisibilityControlPermission(entity));
        }

        protected virtual IEntity GetEntity(int id) => throw new NotSupportedException("This endpoint does not support access rights");

        protected virtual bool AllowCreateNewEntity(int organizationId) => throw new NotSupportedException("This endpoint does not support generic creation rights");

        [HttpGet]
        [InternalApi]
        /// <summary>
        /// GET api/T/GetPermissions
        /// Checks what access rights the user has for the given entities
        /// </summary>
        public virtual HttpResponseMessage GetAccessRights(bool? getEntitiesAccessRights, int organizationId)
        {
            if (GetOrganizationReadAccessLevel(organizationId) == OrganizationDataReadAccessLevel.None)
            {
                return Forbidden();
            }
            return Ok(new EntitiesAccessRightsDTO
            {
                CanCreate = AllowCreateNewEntity(organizationId),
                CanView = true
            });
        }

        [HttpGet]
        [InternalApi]
        /// <summary>
        /// GET api/T/id?GetAccessRightsForEntity
        /// Checks what access rights the user has for the given entity
        /// </summary>
        /// <param name="id">The id of the object</param>
        public virtual HttpResponseMessage GetAccessRightsForEntity(int id, bool? getEntityAccessRights)
        {
            var item = GetEntity(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(GetAccessRightsForEntity(item));
        }

        private EntityAccessRightsDTO GetAccessRightsForEntity(IEntity item)
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
        /// <summary>
        /// POST api/T/idListAsCsv?getEntityListAccessRights
        /// Uses POST verb to allow use of body for potentially long list of ids
        /// Checks what access rights the user has for the given entities identified by the <see cref=""/> list
        /// </summary>
        /// <param name="ids">The ids of the objects</param>
        public virtual HttpResponseMessage PostSearchAccessRightsForEntityList([FromBody] int[] ids, bool? getEntityListAccessRights)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest();
            }

            return Ok(
                ids
                    .Distinct()
                    .Select(GetEntity)
                    .Where(entity => entity != null)
                    .Select(GetAccessRightsForEntity)
                    .ToList()
            );
        }
    }
}
