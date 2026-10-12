using System;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Net;
using Core.DomainModel;
using System.Linq;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class OrganizationsController : BaseEntityController<Organization>
    {
        private readonly IOrganizationService _organizationService;
        private readonly IGenericRepository<User> _userRepository;

        public OrganizationsController(
            IGenericRepository<Organization> repository,
            IOrganizationService organizationService,
            IGenericRepository<User> userRepository)
            : base(repository)
        {
            _organizationService = organizationService;
            _userRepository = userRepository;
        }

        [HttpPost("odata/Organizations({orgKey})/RemoveUser")]
        public IActionResult RemoveUser([FromRoute]int orgKey, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (parameters.ContainsKey("userId"))
            {
                var userId = (int)parameters["userId"];

                var result = _organizationService.RemoveUser(orgKey, userId);
                return
                    result.Ok ?
                        StatusCode((int)HttpStatusCode.NoContent) :
                        FromOperationFailure(result.Error);
            }

            return BadRequest("No user ID specified");
        }

        [NonAction]
        public override IActionResult Post(int organizationId, Organization organization) => throw new NotSupportedException();

        [HttpGet("odata/Organizations({key})/Users")]
        [EnableQuery]
        public IActionResult GetUsers([FromRoute] int key)
        {
            var accessLevel = GetOrganizationReadAccessLevel(key);
            if (accessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }

            var result = _userRepository.AsQueryable().Where(m => m.OrganizationRights.Any(r => r.OrganizationId == key));
            return Ok(result.ToList());
        }

        [NonAction]
        public override IActionResult Patch(int key, Delta<Organization> delta) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Delete(int key) => throw new NotSupportedException();
    }
}



