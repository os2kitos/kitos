﻿using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using System.Web.Http.Results;
using System.Net;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemsController : BaseEntityController<ItSystem>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public ItSystemsController(IGenericRepository<ItSystem> repository, IUserService userService, IAuthenticationService authService)
            : base(repository)
        {
            _userService = userService;
            _authService = authService;
        }

        [EnableQuery(MaxExpansionDepth = 5)]
        [ODataRoute("ItSystems")]
        public override IHttpActionResult Get()
        {
            if (_authService.HasReadAccessOutsideContext(UserId))
            {
                return base.Get();
            }
            else
            {
                var orgId = _userService.GetCurrentOrganizationId(UserId);
                return Ok(Repository.AsQueryable().Where(x => x.OrganizationId == orgId));
            }
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItSystems")]
        public IHttpActionResult GetItSystems(int key)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                {
                    return new StatusCodeResult(HttpStatusCode.Forbidden, this);
                }
                else
                {
                    var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
                    return Ok(result);
                }
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/BelongingSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/BelongingSystems")]
        public IHttpActionResult GetBelongingSystems(int key)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                {
                    return new StatusCodeResult(HttpStatusCode.Forbidden, this);
                }
                else
                {
                    var result = Repository.AsQueryable().Where(m => m.BelongsToId == key);
                    return Ok(result);
                }
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems({sysKey})")]
        public IHttpActionResult GetItSystems(int orgKey, int sysKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == sysKey);
            if (entity == null)
                return NotFound();

            if (_authService.HasReadAccess(UserId, entity))
            {
                return Ok(entity);
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }
        }
    }
}
