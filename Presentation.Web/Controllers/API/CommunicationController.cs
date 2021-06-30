using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class CommunicationController : GenericApiController<Communication, CommunicationDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public CommunicationController(
            IGenericRepository<Communication> repository,
            IItProjectRepository projectRepository)
            : base(repository)
        {
            _projectRepository = projectRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<CommunicationDTO>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetSingle(int id, [FromUri] bool project)
        {
            var itProject = _projectRepository.GetById(id);
            
            if (itProject == null)
                return NotFound();
            
            if (!AllowRead(itProject))
                return Forbidden();

            var items = Repository.Get(x => x.ItProjectId == id);

            return Ok(Map(items));
        }

        protected override IQueryable<Communication> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }

            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => organizationIds.Contains(x.ItProject.OrganizationId));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Communication, ItProject>(x => _projectRepository.GetById(x.ItProjectId), base.GetCrudAuthorization());
        }
    }
}
