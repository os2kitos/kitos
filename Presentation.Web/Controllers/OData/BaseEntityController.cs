using System.Web.Http;
using System.Web.OData;
using Core.DomainServices;
using Core.ApplicationServices;
using System.Net;
using System;
using Core.DomainModel;
using System.Linq;
using Ninject.Infrastructure.Language;
using Presentation.Web.Access;

namespace Presentation.Web.Controllers.OData
{
    public abstract class BaseEntityController<T> : BaseController<T> where T : class, IEntity
    {
        private readonly IOrganizationContextFactory _organizationContextFactory;
        protected IAuthenticationService AuthService { get; } //TODO: Remove once the new aproach is validated

        protected BaseEntityController(
            IGenericRepository<T> repository,
            IAuthenticationService authService,
            IOrganizationContextFactory organizationContextFactory = null)
            : base(repository)
        {
            _organizationContextFactory = organizationContextFactory;
            AuthService = authService;
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            var hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));

            var result = Repository.AsQueryable().ToEnumerable();

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

            result = result.Where(AllowReadAccess);

            return Ok(result.AsQueryable());
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

            var result = Repository.AsQueryable().Where(m => ((IHasOrganization)m).OrganizationId == key);
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
            if (ApplyNewAccessControlScheme())
            {
                var accessContext = _organizationContextFactory.CreateOrganizationAccessContext(organizationId);

                return accessContext.AllowReads(UserId);
            }
            var loggedIntoOrgId = AuthService.GetCurrentOrganizationId(UserId);
            return loggedIntoOrgId == organizationId || AuthService.HasReadAccessOutsideContext(UserId);
        }

        private bool AllowReadAccess(T entity)
        {
            if (ApplyNewAccessControlScheme())
            {
                var organizationId = AuthService.GetCurrentOrganizationId(UserId);
                var accessContext = _organizationContextFactory.CreateOrganizationAccessContext(organizationId);
                return accessContext.AllowReads(UserId, entity);
            }
            return AuthService.HasReadAccess(UserId, entity);
        }

        private bool AllowWriteAccess(T entity)
        {
            if (ApplyNewAccessControlScheme())
            {
                var targetOrganization = GetTargetOrganization(entity);
                var accessContext = _organizationContextFactory.CreateOrganizationAccessContext(targetOrganization);
                return accessContext.AllowUpdates(UserId, entity);
            }
            return AuthService.HasWriteAccess(UserId, entity);
        }

        private bool ApplyNewAccessControlScheme()
        {
            return _organizationContextFactory != null;
        }


        protected int GetTargetOrganization(T entity)
        {
            var targetOrganization = AuthService.GetCurrentOrganizationId(UserId);
            if (entity is IHasOrganization organization)
            {
                if (organization.OrganizationId == 0)
                {
                    organization.OrganizationId = targetOrganization;
                }
                else
                {
                    targetOrganization = organization.OrganizationId;
                }
            }

            return targetOrganization;
        }
    }
}
