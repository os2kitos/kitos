﻿using System.Web.Http;
using Microsoft.AspNet.OData;
using Core.DomainServices;
using System.Net;
using System;
using Core.DomainModel;
using System.Linq;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;
using Ninject;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;

namespace Presentation.Web.Controllers.OData
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

            var mainQuery = Repository.AsQueryable();

            var result = refinement
                .Select(x => x.Apply(mainQuery))
                .GetValueOrFallback(mainQuery);

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
                DomainEvents.Raise(new EntityCreatedEvent<T>(entity));
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
            if (AllowModify(entity) == false)
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
                DomainEvents.Raise(new EntityUpdatedEvent<T>(entity));
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
                DomainEvents.Raise(new EntityDeletedEvent<T>(entity));
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
            switch (failure)
            {
                case OperationFailure.BadInput:
                    return BadRequest();
                case OperationFailure.NotFound:
                    return NotFound();
                case OperationFailure.Forbidden:
                    return Forbidden();
                case OperationFailure.Conflict:
                    return Conflict();
                default:
                    return StatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}
