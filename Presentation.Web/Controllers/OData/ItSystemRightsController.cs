using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Access;
using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Controllers.OData
{
    using System;
    using System.Net;

    public class ItSystemRightsController : BaseEntityController<ItSystemRight>
    {
        private readonly IOrganizationContextFactory _contextFactory;
        private readonly IAuthenticationContext _authenticationContext;

        public ItSystemRightsController(
            IGenericRepository<ItSystemRight> repository,
            IAuthenticationService authService,
            IOrganizationContextFactory contextFactory,
            IAuthenticationContext authenticationContext)
            : base(repository, authService)
        {
            _contextFactory = contextFactory;
            _authenticationContext = authenticationContext;
        }

        // GET /Organizations(1)/ItSystemUsages(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItSystemUsages({usageId})/Rights")]
        public IHttpActionResult GetByItSystem(int orgId, int usageId)
        {
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == usageId).ToList();

            result = FilterByAccessControl(orgId, result);

            return Ok(result);
        }

        // GET /Users(1)/ItProjectRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItSystemRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            var result = Repository.AsQueryable().Where(x => x.UserId == userId).ToList();

            result = FilterByAccessControl(_authenticationContext.ActiveOrganizationId.GetValueOrDefault(-1), result);

            return Ok(result);
        }

        public override IHttpActionResult Patch(int key, Delta<ItSystemRight> delta)
        {
            var entity = Repository.GetByKey(key);

            // check model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // does the entity exist?
            if (entity == null)
            {
                return NotFound();
            }

            // check if user is allowed to write to the entity
            var accessContext = CreateAccessContext(entity);
            if (accessContext.AllowUpdates(UserId, entity) == false)
            {
                return Forbidden();
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

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);

            if (entity == null)
            {
                return NotFound();
            }

            var accessContext = CreateAccessContext(entity);
            if (accessContext.AllowUpdates(UserId, entity) == false)
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

        private List<ItSystemRight> FilterByAccessControl(int orgId, List<ItSystemRight> result)
        {
            //TODO: Org Id should always be the "active context" org ID from which access to certain entities is allowed.
            var accessContext = _contextFactory.CreateOrganizationAccessContext(orgId);

            result = result.Where(x => accessContext.AllowReads(UserId, x)).ToList();
            return result;
        }

        private OrganizationAccessContext CreateAccessContext(ItSystemRight entity)
        {
            var accessContext = _contextFactory.CreateOrganizationAccessContext(entity.Object.OrganizationId);
            return accessContext;
        }
    }
}
