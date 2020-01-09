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
    [MigratedToNewAuthorizationContext]
    public class GoalStatusController : GenericContextAwareApiController<GoalStatus, GoalStatusDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public GoalStatusController(
            IGenericRepository<GoalStatus> repository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository projectRepository
            ) 
            : base(repository, authorizationContext)
        {
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<GoalStatus>(goalStatus=> _projectRepository.GetById(goalStatus.ItProject.Id), base.GetCrudAuthorization());
        }
    }
}
