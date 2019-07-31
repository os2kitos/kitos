using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.ApplicationServices;
using Core.DomainServices;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationRightsController : BaseEntityController<OrganizationRight>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public OrganizationRightsController(IGenericRepository<OrganizationRight> repository, IUserService userService, IAuthenticationService authService)
            : base(repository, authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // GET /Organizations(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/Rights")]
        public IHttpActionResult GetRights(int orgKey)
        {
            var result = Repository.AsQueryable().Where(x => x.OrganizationId == orgKey);
            return Ok(result);
        }

        // POST /Organizations(1)/Rights
        [ODataRoute("Organizations({orgKey})/Rights")]
        public IHttpActionResult PostRights(int orgKey, OrganizationRight entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _userService.GetUserById(UserId);

            if(entity.Role == OrganizationRole.GlobalAdmin)
            {
                if(!user.IsGlobalAdmin)
                {
                    return Unauthorized();
                }
            }

            if(entity.Role == OrganizationRole.LocalAdmin)
            {
                if(!user.IsGlobalAdmin && !user.IsLocalAdmin)
                {
                    return Unauthorized();
                }
            }

            entity.OrganizationId = orgKey;
            entity.ObjectOwnerId = UserId;

            if (!_authService.HasWriteAccess(UserId, entity) && !_authService.IsLocalAdmin(this.UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            entity.LastChangedByUserId = UserId;
            
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

        /// <summary>
        /// Always Unauthorized 401. Use POST /Organizations(orgKey)/Rights instead
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override IHttpActionResult Post(OrganizationRight entity)
        {
            return Unauthorized();
            //return base.Post(entity);
        }

        // DELETE /Organizations(1)/Rights(1)
        [ODataRoute("Organizations({orgKey})/Rights({key})")]
        public IHttpActionResult DeleteRights(int orgKey, int key)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.OrganizationId == orgKey && m.Id == key);
            if (entity == null)
            {
                return NotFound();
            }

            var user = _userService.GetUserById(UserId);

            if (entity.Role == OrganizationRole.GlobalAdmin)
            {
                if (!user.IsGlobalAdmin)
                {
                    return Unauthorized();
                }
            }

            if (entity.Role == OrganizationRole.LocalAdmin)
            {
                if (!user.IsGlobalAdmin && !user.IsLocalAdmin)
                {
                    return Unauthorized();
                }
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

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);
            if (entity == null)
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, entity) && !_authService.IsLocalAdmin(this.UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var user = _userService.GetUserById(UserId);

            if (entity.Role == OrganizationRole.GlobalAdmin)
            {
                if (!user.IsGlobalAdmin)
                {
                    return Unauthorized();
                }
            }

            if (entity.Role == OrganizationRole.LocalAdmin)
            {
                if (!user.IsGlobalAdmin && !user.IsLocalAdmin)
                {
                    return Unauthorized();
                }
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

        public override IHttpActionResult Patch(int key, Delta<OrganizationRight> delta)
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
                return StatusCode(HttpStatusCode.Forbidden);
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
    }
}
