using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainServices;
using Core.DomainModel.ItContract;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    using System;
    using System.Net;

    public class ItContractRightsController : BaseEntityController<ItContractRight>
    {
        private IAuthenticationService _authService;
        public ItContractRightsController(IGenericRepository<ItContractRight> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            this._authService = authService;
        }

        // GET /Organizations(1)/ItContracts(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItContracts({contractId})/Rights")]
        public IHttpActionResult GetByItContract(int orgId, int contractId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == contractId);
            return Ok(result);
        }

        // GET /Users(1)/ItContractRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItContractRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);
            var test = !_authService.IsLocalAdmin(this.UserId);
            if (entity == null)
            {
                return NotFound();
            }

            if (!_authService.HasWriteAccess(UserId, entity) && !_authService.IsLocalAdmin(this.UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
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

        public override IHttpActionResult Patch(int key, Delta<ItContractRight> delta)
        {
            var entity = Repository.GetByKey(key);


            // does the entity exist?
            if (entity == null)
                return NotFound();

            // check if user is allowed to write to the entity
            if (!_authService.HasWriteAccess(UserId, entity) && !_authService.IsLocalAdmin(this.UserId))
                return StatusCode(HttpStatusCode.Forbidden);

            //Check if user is allowed to set accessmodifier to public
            //var accessModifier = (entity as IHasAccessModifier)?.AccessModifier;
            //if (accessModifier == AccessModifier.Public && !AuthService.CanExecute(UserId, Feature.CanSetAccessModifierToPublic))
            //{
            //    return Unauthorized();
            //}

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
    }
}
