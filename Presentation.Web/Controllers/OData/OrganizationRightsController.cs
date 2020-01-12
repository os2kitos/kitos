using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Organizations;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class OrganizationRightsController : BaseEntityController<OrganizationRight>
    {
        private readonly IUserService _userService;
        private readonly IOrganizationRightsService _organizationRightsService;

        public OrganizationRightsController(
            IGenericRepository<OrganizationRight> repository,
            IUserService userService,
            IOrganizationRightsService organizationRightsService)
            : base(repository)
        {
            _userService = userService;
            _organizationRightsService = organizationRightsService;
        }

        // GET /Organizations(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/Rights")]
        public IHttpActionResult GetRights(int orgKey)
        {
            if (GetCrossOrganizationReadAccessLevel() != CrossOrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }
            var result = Repository.AsQueryable().Where(x => x.OrganizationId == orgKey);
            return Ok(result);
        }

        // POST /Organizations(1)/Rights
        [ODataRoute("Organizations({orgKey})/Rights")]
        public IHttpActionResult PostRights(int orgKey, OrganizationRight entity)
        {
            //TODO: Migrate to the service
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _userService.GetUserById(UserId);

            if (entity.Role == OrganizationRole.GlobalAdmin)
            {
                if (!user.IsGlobalAdmin)
                {
                    return Forbidden();
                }
            }

            if (entity.Role == OrganizationRole.LocalAdmin)
            {
                if (!user.IsGlobalAdmin && !user.IsLocalAdmin)
                {
                    return Forbidden();
                }
            }

            entity.OrganizationId = orgKey;
            entity.ObjectOwnerId = UserId;

            if (!AllowCreate<OrganizationRight>(entity))
            {
                return Forbidden();
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
        /// Always Use 403 - POST /Organizations(orgKey)/Rights instead
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override IHttpActionResult Post(OrganizationRight entity)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE /Organizations(1)/Rights(1)
        [ODataRoute("Organizations({orgKey})/Rights({key})")]
        public IHttpActionResult DeleteRights(int orgKey, int key)
        {
            return PerformDelete(key);
        }

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);

            return entity == null ?
                NotFound() :
                PerformDelete(entity.Id);
        }

        private IHttpActionResult PerformDelete(int key)
        {
            try
            {
                var result = _organizationRightsService.RemoveRole(key);

                if (result.Ok)
                {
                    return StatusCode(HttpStatusCode.NoContent);
                }

                switch (result.Error)
                {
                    case GenericOperationFailure.BadInput:
                        return BadRequest();
                    case GenericOperationFailure.NotFound:
                        return NotFound();
                    case GenericOperationFailure.Forbidden:
                        return Forbidden();
                    case GenericOperationFailure.Conflict:
                        return StatusCode(HttpStatusCode.Conflict);
                    default:
                        return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
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
            if (!AllowWrite(entity))
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
    }
}
