﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.ApplicationServices;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    [PublicApi]
    public class ItProjectsController : BaseEntityController<ItProject>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IAuthenticationService _authService;

        public ItProjectsController(IGenericRepository<ItProject> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _orgUnitRepository = orgUnitRepository;
            _authService = authService;
        }

        [EnableQuery]
        [ODataRoute("ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<ItProject>))]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Henter organisationens projekter samt offentlige projekter fra andre organisationer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItProject>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItProjects(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                {
                    return Forbidden();
                }

                var result = Repository.AsQueryable().ByOrganizationId(key);
                return Ok(result);
            }
            else
            {
                var result = Repository.AsQueryable().ByPublicAccessOrOrganizationId(key);
                return Ok(result);
            }
        }

        // TODO for now only read actions are allowed, in future write will be enabled - but keep security in mind!
        // GET /Organizations(1)/OrganizationUnits(1)/ItProjects
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<List<ItProject>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItProjectsByOrgUnit(int orgKey, int unitKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return Forbidden();
            }

            var projects = new List<ItProject>();

            // using iteration instead of recursion else we're running into
            // an "multiple DataReaders open" issue and MySQL doesn't support MARS

            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable()
                    .Include(x => x.Children)
                    .Include(x => x.UsingItProjects.Select(y => y.ResponsibleItProject))
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                var responsibles = orgUnit.UsingItProjects.Select(x => x.ResponsibleItProject).Where(x => x != null);
                projects.AddRange(responsibles);

                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return Ok(projects);
        }
    }
}
