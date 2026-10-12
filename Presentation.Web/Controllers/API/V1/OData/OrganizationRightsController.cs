using System.Linq;
using System;
using System.Net;
using Core.ApplicationServices.Organizations;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
        [HttpGet("odata/Organizations({orgKey})/Rights")]
        public IActionResult GetRights(int orgKey)
        {
            if (GetOrganizationReadAccessLevel(orgKey) != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }
            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey);

            return Ok(result.ToList());
        }

        // POST /Organizations(1)/Rights
        [HttpPost("odata/Organizations({orgKey})/Rights")]
        public IActionResult PostRights(int orgKey, [FromBody] OrganizationRight entity)
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
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Always Use 405 - POST /Organizations(orgKey)/Rights instead
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [NonAction]
        public override IActionResult Post(int organizationId, OrganizationRight entity) => throw new NotSupportedException();

        // DELETE /Organizations(1)/Rights(1)
        [HttpDelete("odata/Organizations({orgKey})/Rights({key})")]
        public IActionResult DeleteRights(int orgKey, int key)
        {
            return PerformDelete(key);
        }

        public override IActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);

            return entity == null ?
                NotFound() :
                PerformDelete(entity.Id);
        }

        private IActionResult PerformDelete(int key)
        {
            try
            {
                var result = _organizationRightsService.RemoveRole(key);

                if (result.Ok)
                {
                    return StatusCode((int)HttpStatusCode.NoContent);
                }

                return FromOperationFailure(result.Error);
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [NonAction]
        public override IActionResult Patch(int key, Delta<OrganizationRight> delta) => throw new NotSupportedException();
    }
}



