using System;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Net;
using System.Security;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Core.DomainModel;
using System.Linq;
using System.Net.Http;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class OrganizationsController : BaseEntityController<Organization>
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly IGenericRepository<User> _userRepository;

        public OrganizationsController(
            IGenericRepository<Organization> repository,
            IOrganizationService organizationService,
            IOrganizationRoleService organizationRoleService,
            IGenericRepository<User> userRepository)
            : base(repository)
        {
            _organizationService = organizationService;
            _organizationRoleService = organizationRoleService;
            _userRepository = userRepository;
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

        [EnableQuery]
        public IHttpActionResult Post(Organization organization)
        {
            if (organization == null)
            {
                return BadRequest();
            }

            var result = _organizationService.CreateNewOrganization(organization);

            return result.Ok ? 
                Created(result.Value) : 
                FromOperationFailure(result.Error);
        }

        [EnableQuery]
        public override IHttpActionResult Post(int organizationId, Organization organization)
        {
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
        }

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

        public override IHttpActionResult Patch(int key, Delta<Organization> delta)
        {

            try
            {
                var organization = delta.GetInstance();
                if (organization.TypeId > 0)
                {
                    var typeKey = (OrganizationTypeKeys)organization.TypeId;
                    if (!_organizationService.CanChangeOrganizationType(organization, typeKey))
                    {
                        return Forbidden();
                    }
                }
            }
            catch (SecurityException e)
            {
                return Forbidden();
            }
            return base.Patch(key, delta);
        }
    }
}
