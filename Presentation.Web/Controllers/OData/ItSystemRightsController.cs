using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    using System;
    using System.Net;

    public class ItSystemRightsController : BaseEntityController<ItSystemRight>
    {
        private IAuthenticationService _authService;
        public ItSystemRightsController(IGenericRepository<ItSystemRight> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            this._authService = authService;
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItSystemUsages({usageId})/Rights")]
        public IHttpActionResult GetByItSystem(int orgId, int usageId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == usageId);
            return Ok(result);
        }

        // GET /Users(1)/ItProjectRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItSystemRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }

        public override IHttpActionResult Patch(int key, Delta<ItSystemRight> delta)
        {
            var entity = Repository.GetByKey(key);

            // does the entity exist?
            if (entity == null)
            {
                return NotFound();
            }

            // check if user is allowed to write to the entity
            if (!_authService.HasWriteAccess(UserId, entity) && !_authService.IsLocalAdmin(this.UserId))
            {
                return Forbidden();
            }

            //Check if user is allowed to set accessmodifier to public
            //var accessModifier = (entity as IHasAccessModifier)?.AccessModifier;
            //if (accessModifier == AccessModifier.Public && !AuthService.CanExecute(UserId, Feature.CanSetAccessModifierToPublic))
            //{
            //    return Unauthorized();
            //}

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

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);

            if (entity == null)
            {
                return NotFound();
            }

            if (!_authService.HasWriteAccess(UserId, entity) && !_authService.IsLocalAdmin(this.UserId))
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
    }
}
