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
    public class OrganizationRightsController : BaseController<OrganizationRight>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public OrganizationRightsController(IGenericRepository<OrganizationRight> repository, IUserService userService, IAuthenticationService authService)
            : base(repository)
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
                return BadRequest(ModelState);

            entity.OrganizationId = orgKey;
            entity.ObjectOwnerId = UserId;
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

        // DELETE /Organizations(1)/Rights(1)
        [ODataRoute("Organizations({orgKey})/Rights({key})")]
        public IHttpActionResult DeleteRights(int orgKey, int key)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.OrganizationId == orgKey && m.Id == key);
            if (entity == null)
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, entity))
                return StatusCode(HttpStatusCode.Forbidden);

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
