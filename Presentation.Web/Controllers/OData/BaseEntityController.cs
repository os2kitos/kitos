﻿using System.Web.Http;
using System.Web.OData;
using Core.DomainServices;
using Core.ApplicationServices;
using System.Net;
using System;
using Core.DomainModel;
using System.Linq;
using Ninject.Infrastructure.Language;
using Presentation.Web.Infrastructure.Authorization;

namespace Presentation.Web.Controllers.OData
{
    public abstract class BaseEntityController<T> : BaseController<T> where T : class, IEntity
    {
        protected IAuthenticationService AuthService { get; } //TODO: Remove once the new approach is validated
        private readonly IControllerAuthorizationStrategy _authorizationStrategy;

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
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            var hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));

            var result = Repository.AsQueryable(readOnly: true).ToEnumerable();

            if (AuthService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
            {
                if (hasAccessModifier && !AuthService.IsGlobalAdmin(UserId))
                {
                    if (hasOrg)
                    {
                        result = result.Where(x => ((IHasAccessModifier)x).AccessModifier == AccessModifier.Public || ((IHasOrganization)x).OrganizationId == AuthService.GetCurrentOrganizationId(UserId));
                    }
                    else
                    {
                        result = result.Where(x => ((IHasAccessModifier)x).AccessModifier == AccessModifier.Public);
                    }
                }
            }
            else
            {
                result = result.Where(x => ((IHasOrganization)x).OrganizationId == AuthService.GetCurrentOrganizationId(UserId));
            }

            if (_authorizationStrategy.ApplyBaseQueryPostProcessing)
            {
                //Post processing was not a part of the old response, so let the migration control when we switch
                result = result.Where(AllowReadAccess);
            }

            return Ok(result.AsQueryable());
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public override IHttpActionResult Get(int key)
        {
            var result = Repository.AsQueryable(readOnly: true).Where(p => p.Id == key);

            if (result.Any() == false)
            {
                return NotFound();
            }

            var entity = result.First();
            if (AllowReadAccess(entity) == false)
            {
                return Forbidden();
            }

            return Ok(SingleResult.Create(result));
        }

        [EnableQuery(MaxExpansionDepth = 5)]
        public IHttpActionResult GetByOrganizationKey(int key)
        {
            if (typeof(IHasOrganization).IsAssignableFrom(typeof(T)) == false)
                throw new InvalidCastException("Entity must implement IHasOrganization");

            if (AllowOrganizationAccess(key) == false)
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable(readOnly: true).Where(m => ((IHasOrganization)m).OrganizationId == key);

            if (_authorizationStrategy.ApplyBaseQueryPostProcessing)
            {
                //Post processing was not a part of the old response, so let the migration control when we switch
                result = result.AsEnumerable().Where(AllowReadAccess).AsQueryable();
            }

            return Ok(result);
        }

        [System.Web.Http.Description.ApiExplorerSettings]
        public virtual IHttpActionResult Post(T entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entity.ObjectOwnerId = UserId;
            entity.LastChangedByUserId = UserId;

            if (AllowWriteAccess(entity) == false)
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

            // does the entity exist?
            if (entity == null)
            {
                return NotFound();
            }

            // check if user is allowed to write to the entity
            if (AllowWriteAccess(entity) == false)
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

            if (AllowWriteAccess(entity) == false)
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

        protected bool AllowOrganizationAccess(int organizationId)
        {
            return _authorizationStrategy.AllowOrganizationAccess(organizationId);
        }

        protected bool AllowReadAccess(T entity)
        {
            return _authorizationStrategy.AllowReadAccess(entity);
        }

        protected bool AllowWriteAccess(T entity)
        {
            return _authorizationStrategy.AllowWriteAccess(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return _authorizationStrategy.AllowEntityVisibilityControl(entity);
        }
    }
}
