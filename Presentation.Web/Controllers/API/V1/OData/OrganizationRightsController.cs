using System;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.ApplicationServices.Organizations;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class OrganizationRightsController : BaseEntityController<OrganizationRight>
    {
        private readonly IOrganizationRightsService _organizationRightsService;

        public OrganizationRightsController(
            IGenericRepository<OrganizationRight> repository,
            IOrganizationRightsService organizationRightsService)
            : base(repository)
        {
            _organizationRightsService = organizationRightsService;
        }

        // GET /Organizations(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/Rights")]
        public IHttpActionResult GetRights(int orgKey)
        {
            if (GetOrganizationReadAccessLevel(orgKey) != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }
            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey);

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

            try
            {
                var result = _organizationRightsService.AssignRole(orgKey, entity.UserId, entity.Role);
                if (result.Ok)
                {
                    return Created(entity);
                }

                return FromOperationFailure(result.Error);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to add right", e);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Always Use 405 - POST /Organizations(orgKey)/Rights instead
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [NonAction]
        public override IHttpActionResult Post(int organizationId, OrganizationRight entity) => throw new NotSupportedException();

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

                return FromOperationFailure(result.Error);
            }
            catch (Exception e)
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<OrganizationRight> delta) => throw new NotSupportedException();
    }
}
