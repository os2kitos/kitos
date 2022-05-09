﻿using System;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Core.DomainModel;
using System.Linq;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Authorization;
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
