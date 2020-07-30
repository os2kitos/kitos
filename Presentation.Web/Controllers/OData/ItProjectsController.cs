using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
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

        public ItProjectsController(IGenericRepository<ItProject> repository, IGenericRepository<OrganizationUnit> orgUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
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
            var all = Repository.AsQueryable();

            if (GetCrossOrganizationReadAccessLevel() < CrossOrganizationDataReadAccessLevel.All)
            {
                if (GetOrganizationReadAccessLevel(key) < OrganizationDataReadAccessLevel.All)
                {
                    return Forbidden();
                }

                var result = all.ByOrganizationId(key);
                return Ok(result);
            }
            else
            {
                var result = all
                    .ByPublicAccessOrOrganizationId(key);

                return Ok(result);
            }
        }

        // GET /Organizations(1)/OrganizationUnits(1)/ItProjects
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItProjects")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<List<ItProject>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItProjectsByOrgUnit(int orgKey, int unitKey)
        {
            if (GetOrganizationReadAccessLevel(orgKey) < OrganizationDataReadAccessLevel.All)
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

        public override IHttpActionResult Delete(int key)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }

        public override IHttpActionResult Post(int organizationId, ItProject entity)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }
    }
}
