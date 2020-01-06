using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class AssignmentController : GenericContextAwareApiController<Assignment, AssignmentDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public AssignmentController(
            IGenericRepository<Assignment> repository,
            IItProjectRepository projectRepository,
            IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Assignment>(x => _projectRepository.GetById(x.AssociatedItProjectId.GetValueOrDefault(-1)), base.GetCrudAuthorization());
        }
    }
}
