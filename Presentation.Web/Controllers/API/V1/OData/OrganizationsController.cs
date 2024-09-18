using System;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Core.DomainModel;
using System.Linq;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItSystemUsage.Read;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using System.Windows.Input;
using System.Security.Cryptography;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class OrganizationsController : BaseEntityController<Organization>
    {
        private readonly IOrganizationService _organizationService;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public OrganizationsController(
            IGenericRepository<Organization> repository,
            IOrganizationService organizationService,
            IGenericRepository<User> userRepository,
            IEntityIdentityResolver entityIdentityResolver)
            : base(repository)
        {
            _organizationService = organizationService;
            _userRepository = userRepository;
            _entityIdentityResolver = entityIdentityResolver;
        }

        [HttpPost]
        public IHttpActionResult RemoveUser([FromODataUri]int orgKey, ODataActionParameters parameters)
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
                        StatusCode(HttpStatusCode.NoContent) :
                        FromOperationFailure(result.Error);
            }

            return BadRequest("No user ID specified");
        }

        [NonAction]
        public override IHttpActionResult Post(int organizationId, Organization organization) => throw new NotSupportedException();

        [EnableQuery]
        public IHttpActionResult GetUsers([FromODataUri] int key)
        {
            var accessLevel = GetOrganizationReadAccessLevel(key);
            if (accessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }

            var result = _userRepository.AsQueryable().Where(m => m.OrganizationRights.Any(r => r.OrganizationId == key));
            return Ok(result);
        }

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<Organization> delta) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();
    }
}
