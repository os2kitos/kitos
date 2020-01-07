using System.Web.Http;
using System.Web.OData;
using Core.DomainServices;
using Core.ApplicationServices;
using System.Net;
using System;
using Core.DomainModel;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;

namespace Presentation.Web.Controllers.OData
{
    [ControllerEvaluationCompleted]
    public abstract class BaseEntityController<T> : BaseController<T> where T : class, IEntity
    {
        protected IAuthenticationService AuthService { get; } //TODO: Remove once the new approach is validated
        private readonly IControllerAuthorizationStrategy _authorizationStrategy;
        private readonly Lazy<IControllerCrudAuthorization> _crudAuthorization;
        protected IControllerCrudAuthorization CrudAuthorization => _crudAuthorization.Value;

        protected BaseEntityController(
            IGenericRepository<T> repository,
            IAuthenticationService authService,
            IAuthorizationContext authorizationContext = null)
            : base(repository)
        {
            _authorizationStrategy =
                authorizationContext == null
                    ? (IControllerAuthorizationStrategy)new LegacyAuthorizationStrategy(authService, () => UserId)
                    : new ContextBasedAuthorizationStrategy(authorizationContext);
            AuthService = authService;
            _crudAuthorization = new Lazy<IControllerCrudAuthorization>(GetCrudAuthorization);
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            var organizationId = AuthService.GetCurrentOrganizationId(UserId);

            var crossOrganizationReadAccess = GetCrossOrganizationReadAccessLevel();

            var refinement = new QueryAllByRestrictionCapabilities<T>(crossOrganizationReadAccess, organizationId);

            var result = refinement.Apply(Repository.AsQueryable());

            if (refinement.RequiresPostFiltering())
            {
                result = result.AsEnumerable().Where(AllowRead).AsQueryable();
            }

            return Ok(result);
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public override IHttpActionResult Get(int key)
        {
            var result = Repository.AsQueryable().Where(p => p.Id == key);

            if (result.Any() == false)
            {
                return NotFound();
            }

            var entity = result.First();
            if (AllowRead(entity) == false)
            {
                return Forbidden();
            }

            return Ok(SingleResult.Create(result));
        }

        [EnableQuery(MaxExpansionDepth = 5)]
        public IHttpActionResult GetByOrganizationKey(int key)
        {
            if (typeof(IHasOrganization).IsAssignableFrom(typeof(T)) == false)
            {
                return BadRequest("Entity does not belong to an organization");
            }

            var accessLevel = GetOrganizationReadAccessLevel(key);

            if (accessLevel == OrganizationDataReadAccessLevel.None)
            {
                return Forbidden();
            }

            var entities = QueryFactory.ByOrganizationId<T>(key, accessLevel).Apply(Repository.AsQueryable());

            return Ok(entities);
        }

        [System.Web.Http.Description.ApiExplorerSettings]
        public virtual IHttpActionResult Post(T entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Make sure organization dependent entity is assigned to the active organization if no explicit organization is provided
            if (entity is IHasOrganization organization && organization.OrganizationId == 0)
            {
                organization.OrganizationId = AuthService.GetCurrentOrganizationId(UserId);
            }

            entity.ObjectOwnerId = UserId;
            entity.LastChangedByUserId = UserId;

            if (AllowCreate<T>(entity) == false)
            {
                return Forbidden();
            }

            if ((entity as IHasAccessModifier)?.AccessModifier == AccessModifier.Public && AllowEntityVisibilityControl(entity) == false)
            {
                return Forbidden();
            }

            try
            {
                entity = Repository.Insert(entity);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Created(entity);
        }

        [System.Web.Http.Description.ApiExplorerSettings]
        public virtual IHttpActionResult Patch(int key, Delta<T> delta)
        {
            var entity = Repository.GetByKey(key);

            if (delta == null)
            {
                return BadRequest();
            }
            // does the entity exist?
            if (entity == null)
            {
                return NotFound();
            }

            // check if user is allowed to write to the entity
            if (AllowWrite(entity) == false)
            {
                return Forbidden();
            }

            if (delta.TryGetPropertyValue(nameof(IHasAccessModifier.AccessModifier), out object accessModifier) &&
                accessModifier.Equals(AccessModifier.Public) && AllowEntityVisibilityControl(entity) == false)
            {
                return Forbidden();
            }

            // check model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // patch the entity
                delta.Patch(entity);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            // add the request header "Prefer: return=representation"
            // if you want the updated entity returned,
            // else you'll just get 204 (No Content) returned
            return Updated(entity);
        }

        public virtual IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);
            if (entity == null)
            {
                return NotFound();
            }

            if (AllowDelete(entity) == false)
            {
                return Forbidden();
            }

            try
            {
                Repository.DeleteByKey(key);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccessLevel()
        {
            return _authorizationStrategy.GetCrossOrganizationReadAccess();
        }

        protected OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return _authorizationStrategy.GetOrganizationReadAccessLevel(organizationId);
        }

        protected bool AllowRead(T entity)
        {
            return CrudAuthorization.AllowRead(entity);
        }

        protected bool AllowWrite(T entity)
        {
            return CrudAuthorization.AllowModify(entity);
        }

        protected bool AllowCreate<T>()
        {
            return _authorizationStrategy.AllowCreate<T>();
        }

        protected bool AllowCreate<T>(IEntity entity)
        {
            return CrudAuthorization.AllowCreate<T>(entity);
        }

        protected bool AllowDelete(IEntity entity)
        {
            return CrudAuthorization.AllowDelete(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authorizationStrategy.AllowEntityVisibilityControl(entity);
        }

        protected virtual IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new RootEntityCrudAuthorization(_authorizationStrategy);
        }
    }
}
