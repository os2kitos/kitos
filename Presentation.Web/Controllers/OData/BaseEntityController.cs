using System.Web.Http;
using System.Web.OData;
using Core.DomainServices;
using Core.ApplicationServices;
using System.Net;
using System;
using Core.DomainModel;
using System.Linq;
using Ninject.Infrastructure.Language;

namespace Presentation.Web.Controllers.OData
{
    public abstract class BaseEntityController<T> : BaseController <T> where T : class, IEntity
    {
        protected readonly IAuthenticationService _authService;

        protected BaseEntityController(IGenericRepository<T> repository, IAuthenticationService authService)
            : base(repository)
        {
            _authService = authService;
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            var hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));

            var result = Repository.AsQueryable();

            if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
            {
                if (hasAccessModifier && !_authService.IsGlobalAdmin(UserId))
                {
                    if (hasOrg)
                    {
                        result = result.ToEnumerable().Where(x => ((IHasAccessModifier)x).AccessModifier == AccessModifier.Public || ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId)).AsQueryable();
                    }
                    else
                    {
                        result = result.ToEnumerable().Where(x => ((IHasAccessModifier)x).AccessModifier == AccessModifier.Public).AsQueryable();
                    }
                }
            }
            else
            {
                result = result.ToEnumerable().Where(x => ((IHasOrganization) x).OrganizationId == _authService.GetCurrentOrganizationId(UserId)).AsQueryable();
            }

            return Ok(result);
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public override IHttpActionResult Get(int key)
        {
            var result = Repository.AsQueryable().Where(p => p.Id == key);

            if (!result.Any())
                return NotFound();

            var entity = result.First();
            if (!_authService.HasReadAccess(UserId, entity))
                return Unauthorized();

            return Ok(SingleResult.Create(result));
        }

        [EnableQuery(MaxExpansionDepth = 5)]
        public IHttpActionResult GetByOrganizationKey(int key)
        {
            if (typeof(IHasOrganization).IsAssignableFrom(typeof(T)) == false)
                throw new InvalidCastException("Entity must implement IHasOrganization");

            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != key && !_authService.HasReadAccessOutsideContext(UserId))
                return Unauthorized();

            var result = Repository.AsQueryable().Where(m => ((IHasOrganization)m).OrganizationId == key);
            return Ok(result);
        }

        [System.Web.Http.Description.ApiExplorerSettings]
        public virtual IHttpActionResult Post(T entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (entity is IHasOrganization && (entity as IHasOrganization).OrganizationId == 0)
            {
                (entity as IHasOrganization).OrganizationId = _authService.GetCurrentOrganizationId(UserId);
            }

            entity.ObjectOwnerId = UserId;
            entity.LastChangedByUserId = UserId;

            if (!_authService.HasWriteAccess(UserId, entity))
            {
                return Unauthorized();
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
                return NotFound();

            // check if user is allowed to write to the entity
            if (!_authService.HasWriteAccess(UserId, entity))
                return StatusCode(HttpStatusCode.Forbidden);

            // check model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, entity))
                return Unauthorized();

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
    }
}
