using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItProjectStatusController : GenericContextAwareApiController<ItProjectStatus, ItProjectStatusDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public ItProjectStatusController(
            IGenericRepository<ItProjectStatus> repository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository projectRepository)
            : base(repository, authorizationContext)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItProjectStatus>(x => _projectRepository.GetById(x.AssociatedItProjectId.GetValueOrDefault(-1)), base.GetCrudAuthorization());
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectStatusDTO>>))]
        public HttpResponseMessage GetByProject(int id, [FromUri] bool? project, [FromUri] PagingModel<ItProjectStatus> paging)
        {
            try
            {
                var itProject = _projectRepository.GetById(id);

                if (itProject == null)
                {
                    return NotFound();
                }

                if (!AllowRead(itProject))
                {
                    return Forbidden();
                }

                var query = Repository.AsQueryable().Where(x => x.AssociatedItProjectId == id);
                var pagedQuery = Page(query, paging);

                return Ok(Map(pagedQuery));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
