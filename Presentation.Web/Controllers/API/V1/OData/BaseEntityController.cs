using System.Web.Http;
using Microsoft.AspNet.OData;
using Core.DomainServices;
using System.Net;
using System;
using Core.DomainModel;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel.Events;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Ninject;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;
using System.Net.Http;

namespace Presentation.Web.Controllers.API.V1.OData
{
    public abstract class BaseEntityController<T> : BaseController<T> where T : class, IEntity
    {
        [Inject]
        public IDomainEvents DomainEvents { get; set; }

        private readonly Lazy<IControllerAuthorizationStrategy> _authorizationStrategy;
        private readonly Lazy<IControllerCrudAuthorization> _crudAuthorization;
        protected IControllerCrudAuthorization CrudAuthorization => _crudAuthorization.Value;

        protected BaseEntityController(IGenericRepository<T> repository)
            : base(repository)
        {
            _authorizationStrategy = new Lazy<IControllerAuthorizationStrategy>(() => new ContextBasedAuthorizationStrategy(AuthorizationContext));
            _crudAuthorization = new Lazy<IControllerCrudAuthorization>(GetCrudAuthorization);
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            var organizationIds = UserContext.OrganizationIds;

            var crossOrganizationReadAccess = GetCrossOrganizationReadAccessLevel();

            var entityAccessLevel = GetEntityTypeReadAccessLevel<T>();

            var refinement = entityAccessLevel == EntityReadAccessLevel.All ?
                Maybe<QueryAllByRestrictionCapabilities<T>>.None :
                Maybe<QueryAllByRestrictionCapabilities<T>>.Some(new QueryAllByRestrictionCapabilities<T>(crossOrganizationReadAccess, organizationIds));

            var mainQuery = GetAllQuery();

            var result = refinement
                .Select(x => x.Apply(mainQuery))
                .GetValueOrFallback(mainQuery);

            return Ok(result);
        }

        protected virtual IQueryable<T> GetAllQuery()
        {
            return Repository.AsQueryable();
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

        [System.Web.Http.Description.ApiExplorerSettings]
        public virtual IHttpActionResult Post(int organizationId, T entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Make sure organization dependent entity is assigned to the active organization if no explicit organization is provided
            if (entity is IOwnedByOrganization organization && organization.OrganizationId == 0)
            {
                organization.OrganizationId = organizationId;
            }

            if (AllowCreate<T>(organizationId, entity) == false)
            {
                return Forbidden();
            }

            try
            {
                entity = Repository.Insert(entity);
                RaiseCreatedDomainEvent(entity);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Created(entity);
        }

        protected virtual void RaiseCreatedDomainEvent(T entity)
        {
            DomainEvents.Raise(new EntityCreatedEvent<T>(entity));
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

            var validationError = ValidatePatch(delta, entity);

            return validationError.Match(error => error, () =>
            {
                try
                {
                    // patch the entity
                    delta.Patch(entity);
                    RaiseUpdatedDomainEvent(entity);
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
            });
        }

        protected virtual Maybe<IHttpActionResult> ValidatePatch(Delta<T> delta, T entity)
        {
            // check if user is allowed to write to the entity
            if (AllowModify(entity) == false)
            {
                {
                    return Forbidden();
                }
            }

            if (delta.TryGetPropertyValue(nameof(IHasAccessModifier.AccessModifier), out object accessModifier) && accessModifier.Equals(AccessModifier.Public) && AllowEntityVisibilityControl(entity) == false)
            {
                {
                    return Forbidden();
                }
            }

            if (entity is IHasUuid hasUuid &&
                delta.GetChangedPropertyNames().Contains(nameof(IHasUuid.Uuid)) &&
                delta.TryGetPropertyValue(nameof(IHasUuid.Uuid), out var uuid) &&
                ((Guid)uuid) != hasUuid.Uuid)
            {
                return BadRequest("UUID cannot be changed");
            }

            // check model state
            if (!ModelState.IsValid)
            {
                {
                    return BadRequest(ModelState);
                }
            }

            return Maybe<IHttpActionResult>.None;
        }

        protected virtual void RaiseUpdatedDomainEvent(T entity)
        {
            DomainEvents.Raise(new EntityUpdatedEvent<T>(entity));
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
                RaiseDeletedDomainEvent(entity);
                Repository.DeleteByKey(key);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected virtual void RaiseDeletedDomainEvent(T entity)
        {
            DomainEvents.Raise(new EntityBeingDeletedEvent<T>(entity));
        }

        protected CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccessLevel()
        {
            return _authorizationStrategy.Value.GetCrossOrganizationReadAccess();
        }

        protected EntityReadAccessLevel GetEntityTypeReadAccessLevel<T>()
        {
            return _authorizationStrategy.Value.GetEntityTypeReadAccessLevel<T>();
        }

        protected OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return _authorizationStrategy.Value.GetOrganizationReadAccessLevel(organizationId);
        }

        protected bool AllowRead(T entity)
        {
            return CrudAuthorization.AllowRead(entity);
        }

        protected bool AllowModify(T entity)
        {
            return CrudAuthorization.AllowModify(entity);
        }

        protected bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return CrudAuthorization.AllowCreate<T>(organizationId, entity);
        }

        protected bool AllowDelete(IEntity entity)
        {
            return CrudAuthorization.AllowDelete(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authorizationStrategy.Value.HasPermission(new VisibilityControlPermission(entity));
        }

        protected virtual IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new RootEntityCrudAuthorization(_authorizationStrategy.Value);
        }

        protected IHttpActionResult FromOperationFailure(OperationFailure failure)
        {
            return StatusCode(failure.ToHttpStatusCode());
        }

        protected IHttpActionResult FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();

            return ResponseMessage(new HttpResponseMessage(statusCode) { Content = new StringContent(failure.Message.GetValueOrFallback(statusCode.ToString("G"))) });
        }
    }
}
