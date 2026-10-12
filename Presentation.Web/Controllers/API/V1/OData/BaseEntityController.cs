using System;
using System.Linq;
using System.Net;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Microsoft.AspNetCore.OData.Results;
using Presentation.Web.Infrastructure.Authorization.Controller.General;

namespace Presentation.Web.Controllers.API.V1.OData
{
    public abstract class BaseEntityController<T> : BaseController<T> where T : class, IEntity
    {
        protected IDomainEvents DomainEvents => HttpContext.RequestServices.GetRequiredService<IDomainEvents>();

        protected IControllerAuthorizationStrategy AuthorizationStrategy =>
            new ContextBasedAuthorizationStrategy(AuthorizationContext);
        protected IControllerCrudAuthorization CrudAuthorization => GetCrudAuthorization();

        protected BaseEntityController(IGenericRepository<T> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        [RequireTopOnOdataThroughKitosToken]
        public override IActionResult Get()
        {
            var organizationIds = UserContext!.OrganizationIds;
            var crossOrganizationReadAccess = GetCrossOrganizationReadAccessLevel();
            var entityAccessLevel = GetEntityTypeReadAccessLevel<T>();

            var refinement = entityAccessLevel == EntityReadAccessLevel.All ?
                Maybe<QueryAllByRestrictionCapabilities<T>>.None :
                Maybe<QueryAllByRestrictionCapabilities<T>>.Some(new QueryAllByRestrictionCapabilities<T>(crossOrganizationReadAccess, organizationIds));

            var mainQuery = GetAllQuery();

            var result = refinement
                .Select(x => x.Apply(mainQuery))
                .GetValueOrFallback(mainQuery);

            // AsEnumerable() materializes via EF6 before OData applies in-memory filtering.
            // [EnableQuery] with IEnumerable uses LINQ-to-Objects, avoiding EF6/OData expression-tree incompatibilities.
            return Ok(result.ToList());
        }

        protected virtual IQueryable<T> GetAllQuery()
        {
            return Repository.AsQueryable();
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public override IActionResult Get(int key)
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

            return Ok(entity);
        }

        public virtual IActionResult Post(int organizationId, [FromBody] T entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
                return StatusCode(500, e.Message);
            }

            return Created(entity);
        }

        protected virtual void RaiseCreatedDomainEvent(T entity)
        {
            DomainEvents.Raise(new EntityCreatedEvent<T>(entity));
        }

        public virtual IActionResult Patch(int key, Delta<T> delta)
        {
            var entity = Repository.GetByKey(key);

            if ((Delta<T>?)delta == null)
            {
                return BadRequest();
            }
            if (entity == null)
            {
                return NotFound();
            }

            var validationError = ValidatePatch(delta, entity);

            return validationError.Match(error => error, () =>
            {
                try
                {
                    delta.Patch(entity);
                    RaiseUpdatedDomainEvent(entity);
                    Repository.Save();
                }
                catch (Exception e)
                {
                    return StatusCode(500, e.Message);
                }

                return Updated(entity);
            });
        }

        protected virtual Maybe<IActionResult> ValidatePatch(Delta<T> delta, T entity)
        {
            if (AllowModify(entity) == false)
            {
                return Maybe<IActionResult>.Some(Forbidden());
            }

            if (delta.TryGetPropertyValue(nameof(IHasAccessModifier.AccessModifier), out object accessModifier) &&
                accessModifier.Equals(AccessModifier.Public) &&
                AllowEntityVisibilityControl(entity) == false)
            {
                return Maybe<IActionResult>.Some(Forbidden());
            }

            if (entity is IHasUuid hasUuid &&
                delta.GetChangedPropertyNames().Contains(nameof(IHasUuid.Uuid)) &&
                delta.TryGetPropertyValue(nameof(IHasUuid.Uuid), out var uuid) &&
                ((Guid)uuid) != hasUuid.Uuid)
            {
                return Maybe<IActionResult>.Some(BadRequest("UUID cannot be changed"));
            }

            if (!ModelState.IsValid)
            {
                return Maybe<IActionResult>.Some(BadRequest(ModelState));
            }

            return Maybe<IActionResult>.None;
        }

        protected virtual void RaiseUpdatedDomainEvent(T entity)
        {
            DomainEvents.Raise(new EntityUpdatedEvent<T>(entity));
        }

        public virtual IActionResult Delete(int key)
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
                return StatusCode(500, e.Message);
            }

            return StatusCode((int)HttpStatusCode.NoContent);
        }

        protected virtual void RaiseDeletedDomainEvent(T entity)
        {
            DomainEvents.Raise(new EntityBeingDeletedEvent<T>(entity));
        }

        protected CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccessLevel()
        {
            return AuthorizationStrategy.GetCrossOrganizationReadAccess();
        }

        protected EntityReadAccessLevel GetEntityTypeReadAccessLevel<TEntity>()
        {
            return AuthorizationStrategy.GetEntityTypeReadAccessLevel<TEntity>();
        }

        protected OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return AuthorizationStrategy.GetOrganizationReadAccessLevel(organizationId);
        }

        protected bool AllowRead(T entity)
        {
            return CrudAuthorization.AllowRead(entity);
        }

        protected bool AllowModify(T entity)
        {
            return CrudAuthorization.AllowModify(entity);
        }

        protected bool AllowCreate<TEntity>(int organizationId, IEntity entity)
        {
            return CrudAuthorization.AllowCreate<TEntity>(organizationId, entity);
        }

        protected bool AllowDelete(IEntity entity)
        {
            return CrudAuthorization.AllowDelete(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return AuthorizationStrategy.HasPermission(new VisibilityControlPermission(entity));
        }

        protected virtual IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new RootEntityCrudAuthorization(AuthorizationStrategy);
        }

        protected IActionResult FromOperationFailure(OperationFailure failure)
        {
            return StatusCode((int)failure.ToHttpStatusCode());
        }

        protected IActionResult FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();
            return StatusCode((int)statusCode, failure.Message.GetValueOrFallback(statusCode.ToString("G")));
        }
    }
}

