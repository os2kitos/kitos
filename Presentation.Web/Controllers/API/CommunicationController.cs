using System.Collections.Generic;
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
    public class CommunicationController : GenericContextAwareApiController<Communication, CommunicationDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public CommunicationController(
            IGenericRepository<Communication> repository,
            IAuthorizationContext authorization,
            IItProjectRepository projectRepository)
            : base(repository, authorization)
        {
            _projectRepository = projectRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<CommunicationDTO>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetSingle(int id, [FromUri] bool project)
        {
            var item = Repository.Get(x => x.ItProjectId == id);

            if (item == null)
                return NotFound();

            return Ok(Map(item));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Communication>(x => _projectRepository.GetById(x.ItProjectId), base.GetCrudAuthorization());
        }
    }
}
