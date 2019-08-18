using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Presentation.Web.Access;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    using System;
    using System.Net;

    [PublicApi]
    public class ItSystemRightsController : BaseEntityController<ItSystemRight>
    {
        public ItSystemRightsController(
            IGenericRepository<ItSystemRight> repository,
            IAuthenticationService authService,
            IOrganizationContextFactory contextFactory)
            : base(repository, authService, contextFactory.CreateOrganizationAccessContext())
        {
        }

        // GET /Organizations(1)/ItSystemUsages(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItSystemUsages({usageId})/Rights")]
        public IHttpActionResult GetByItSystem(int orgId, int usageId)
        {
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == usageId).ToList();

            result = FilterByAccessControl(result);

            return Ok(result.AsQueryable());
        }

        // GET /Users(1)/ItProjectRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItSystemRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            var result = Repository.AsQueryable().Where(x => x.UserId == userId).ToList();

            result = FilterByAccessControl(result);

            return Ok(result.AsQueryable());
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
            if (AccessContext.AllowUpdates(entity) == false)
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

            if (AccessContext.AllowUpdates(entity) == false)
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

        private List<ItSystemRight> FilterByAccessControl(List<ItSystemRight> result)
        {
            result = result.Where(AccessContext.AllowReads).ToList();
            return result;
        }
    }
}
