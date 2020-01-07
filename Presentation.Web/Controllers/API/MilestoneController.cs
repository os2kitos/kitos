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
    public class MilestoneController : GenericContextAwareApiController<Milestone, MilestoneDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public MilestoneController(
            IGenericRepository<Milestone> repository,
            IAuthorizationContext authorization,
            IItProjectRepository projectRepository)
            : base(repository, authorization)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Milestone>(x => _projectRepository.GetById(x.AssociatedItProjectId.GetValueOrDefault(-1)), base.GetCrudAuthorization());
        }
    }
}
